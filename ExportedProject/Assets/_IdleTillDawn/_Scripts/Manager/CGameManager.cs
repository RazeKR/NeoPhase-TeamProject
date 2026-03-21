using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환과 전역 게임 상태를 관리하는 최상위 매니저
/// DontDestroyOnLoad로 씬이 바뀌어도 파괴되지 않으며 데이터 허브 역할만 담당한다
/// 싱글톤이지만 씬 내 세부 로직에 직접 관여하지 않고 데이터 전달 및 씬 전환에만 집중한다
/// 싱글톤 남용을 방지하기 위해 이 클래스 하나에만 싱글톤 패턴을 적용한다
/// </summary>
public class CGameManager : MonoBehaviour
{
    #region Singleton

    public static CGameManager Instance { get; private set; } // 전역 접근점, 씬 전환 후에도 유지

    /// <summary>
    /// 씬 로드 전 가장 먼저 실행된다
    /// CGameManager 인스턴스가 없으면 Resources/CGameManager 프리팹을 자동으로 생성한다
    /// 덕분에 어떤 씬에서 Play를 시작해도 CGameManager.Instance가 null이 되지 않는다
    /// 프리팹 경로: Assets/Resources/CGameManager.prefab
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return; // 이미 존재하면 생성 불필요

        // Resources 폴더 기준 상대경로 — Assets/_IdleTillDawn/Resources/_Prefabs/GameManager_KSH.prefab
        GameObject prefab = Resources.Load<GameObject>("_Prefabs/GameManager_KSH");
        if (prefab == null)
        {
            Debug.LogWarning("[CGameManager] Resources/_Prefabs/GameManager_KSH 프리팹을 찾을 수 없습니다. 경로를 확인하세요.");
            new GameObject("GameManager_KSH").AddComponent<CGameManager>();
            return;
        }

        Instantiate(prefab); // Awake에서 Instance 등록 및 DontDestroyOnLoad 처리
    }

    #endregion

    #region Events

    /// <summary>
    /// 스테이지 인덱스가 증가한 직후 발행된다
    /// 씬 리로드 이전 시점에 발행되므로 CUIManager가 즉시 다음 스테이지 정보를 반영할 수 있다
    /// 인자로 새로운 스테이지 데이터를 전달하여 구독자가 별도 조회 없이 바로 사용할 수 있도록 한다
    /// </summary>
    public event Action<CStageData> OnStageIndexChanged; // (새 스테이지 데이터) — CUIManager가 구독

    #endregion

    #region Inspector Variables

    [Header("스테이지 데이터 목록 (모든 스테이지 SO 연결)")]
    [SerializeField] private CStageData[] _allStageData; // 전체 스테이지 SO 배열 (인덱스 순 정렬 필요)

    [Header("월드별 씬 이름")]
    [SerializeField] private string[] _worldSceneNames; // world[0]="World_1", world[1]="World_2"...

    [Header("스탯 스케일링 공식")]
    [SerializeField] private float _hpGrowthRate     = 0.1f; // 스테이지당 HP 증가율 (기본 10%)
    [SerializeField] private float _bossHpGrowthRate = 0.2f; // 스테이지당 보스 HP 증가율 (기본 20%)
    [SerializeField] private float _bossAtkGrowthRate= 0.15f;// 스테이지당 보스 공격력 증가율 (기본 15%)

    #endregion

    #region Private Variables

    private int currentStageIndex; // 현재 진행 중인 스테이지 인덱스 (0-based, 씬 간 유지)

    #endregion

    #region Properties

    /// <summary>
    /// 현재 스테이지의 SO 데이터를 반환한다
    /// 씬 내 다른 매니저들이 Start에서 이 프로퍼티를 호출하여 스테이지 설정을 받아간다
    /// </summary>
    /// <summary>
    /// 현재 스테이지의 SO 데이터를 반환한다
    /// _allStageData가 비어있거나 인덱스 초과 시 null을 반환하고 오류 원인을 로그로 알린다
    /// </summary>
    public CStageData CurrentStageData
    {
        get
        {
            if (_allStageData == null || _allStageData.Length == 0)
            {
                Debug.LogError("[CGameManager] _allStageData 배열이 비어있습니다. 프리팹 Inspector에서 StageData SO를 연결하세요.");
                return null;
            }
            if (currentStageIndex >= _allStageData.Length)
            {
                Debug.LogError($"[CGameManager] currentStageIndex({currentStageIndex})가 배열 범위({_allStageData.Length})를 초과합니다. 인덱스를 0으로 초기화합니다.");
                currentStageIndex = 0; // 안전하게 첫 스테이지로 복구
            }
            return _allStageData[currentStageIndex];
        }
    }

    #endregion

    #region Unity Methods

    /// <summary>
    /// 씬 중복 로드 방지 및 DontDestroyOnLoad 등록
    /// 두 번째 로드 시 자신을 즉시 파괴하여 인스턴스를 단일 상태로 유지한다
    /// </summary>
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress(); // 시작 시 저장 데이터 불러오기
    }

    /// <summary>
    /// 앱 종료 시점에 현재 stageIndex를 저장한다
    /// 보스 처치 여부와 무관하게 마지막으로 게임을 종료한 시점의 스테이지가 기록된다
    /// OnApplicationQuit은 에디터 종료(플레이 중지), 빌드 종료 모두에서 호출된다
    /// </summary>
    private void OnApplicationQuit() => SaveProgress(); // 종료 시점 저장

    #endregion

    #region Public Methods

    /// <summary>
    /// 보스 처치 후 다음 스테이지로 이동한다
    /// X-10 클리어 시 다음 월드 씬을 로드하고, 그 외에는 인덱스 증가 후 현재 씬을 리로드한다
    /// 씬 리로드 방식으로 씬 내 모든 매니저와 상태를 완전히 초기화한다
    /// </summary>
    public void ProgressToNextStage()
    {
        CStageData current = CurrentStageData;

        if (current.IsLastStage)
        {
            LoadNextWorld(current._world); // 씬 전환 필요
            return;
        }

        currentStageIndex++;                                                 // 다음 스테이지 인덱스
        OnStageIndexChanged?.Invoke(CurrentStageData);                       // 인덱스 증가 직후 UI 즉시 갱신
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);          // 씬 리로드로 상태 초기화
    }

    /// <summary>
    /// 플레이어 사망 시 동일 스테이지를 재시작한다
    /// 스테이지 인덱스는 그대로 두고 씬만 리로드하여 리스폰을 구현한다
    /// 별도 리스폰 로직 없이 씬 재시작으로 모든 상태가 자동 초기화된다
    /// </summary>
    public void RespawnCurrentStage() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 동일 스테이지 재시작

    /// <summary>
    /// 현재 진행 데이터를 PlayerPrefs에 저장한다
    /// 이 메서드만 교체하면 JSON 직렬화, 클라우드 저장 등으로 확장 가능하도록 분리해두었다
    /// 스테이지 클리어 시점에 즉시 호출하여 데이터 유실을 방지한다
    /// </summary>
    public void SaveProgress() =>
        PlayerPrefs.SetInt("StageIndex", currentStageIndex); // 추후 암호화 및 JSON으로 확장 가능

    /// <summary>
    /// 저장된 진행 데이터를 불러온다
    /// 저장 데이터가 없으면 기본값 0(첫 스테이지)으로 시작한다
    /// </summary>
    /// <summary>
    /// 저장된 진행 데이터를 불러온다
    /// 저장값이 배열 범위를 초과하면 0으로 초기화하여 크래시를 방지한다
    /// </summary>
    public void LoadProgress()
    {
        int saved = PlayerPrefs.GetInt("StageIndex", 0);

        // 배열 미연결 상태이면 일단 저장값 그대로 유지 (CurrentStageData에서 재검증)
        if (_allStageData == null || _allStageData.Length == 0)
        {
            currentStageIndex = saved;
            return;
        }

        // 저장값이 배열 범위 초과 시 0으로 복구 (SO 개수 변경 등으로 불일치 발생 방지)
        currentStageIndex = saved < _allStageData.Length ? saved : 0;
    }

    /// <summary>
    /// 현재 stageIndex 기반으로 일반 몬스터 HP 배율을 반환한다
    /// 공식: 1 + (stageIndex × hpGrowthRate)
    /// stageIndex가 씬 전환 후에도 CGameManager에 누적 유지되므로
    /// 스테이지가 오를수록 자동으로 강해지는 "계승 구조"가 성립한다
    /// 예) Stage 1-1(0): 1.0x / Stage 2-1(10): 2.0x / Stage 3-1(20): 3.0x
    /// </summary>
    public float GetEnemyHpMultiplier() =>
        1f + (_hpGrowthRate * currentStageIndex); // 선형 누적 스케일링

    /// <summary>
    /// 현재 stageIndex 기반으로 보스 HP 배율을 반환한다
    /// 일반 몬스터보다 가파른 증가율을 적용하여 보스 차별화를 유지한다
    /// 예) Stage 1-1(0): 1.0x / Stage 2-1(10): 3.0x / Stage 3-1(20): 5.0x
    /// </summary>
    public float GetBossHpMultiplier() =>
        1f + (_bossHpGrowthRate * currentStageIndex); // 보스 HP 누적 스케일링

    /// <summary>
    /// 현재 stageIndex 기반으로 보스 공격력 배율을 반환한다
    /// HP보다 낮은 증가율로 설정하여 공격 패턴 위협도를 점진적으로 상승시킨다
    /// 예) Stage 1-1(0): 1.0x / Stage 2-1(10): 2.5x / Stage 3-1(20): 4.0x
    /// </summary>
    public float GetBossAtkMultiplier() =>
        1f + (_bossAtkGrowthRate * currentStageIndex); // 보스 ATK 누적 스케일링

    #endregion

    #region Private Methods

    /// <summary>
    /// 다음 월드의 씬을 로드한다
    /// worldSceneNames 배열의 범위를 벗어나면 엔딩 처리로 분기한다
    /// 새 월드의 첫 스테이지(X-1) 인덱스로 currentStageIndex를 갱신한 뒤 씬을 로드한다
    /// </summary>
    /// <param name="currentWorld">방금 클리어한 현재 월드 번호 (1-based)</param>
    private void LoadNextWorld(int currentWorld)
    {
        int nextWorldIndex = currentWorld; // currentWorld=1 → 다음 월드 배열 인덱스 1

        if (nextWorldIndex >= _worldSceneNames.Length)
        {
            Debug.Log("[GameManager_KSH] 모든 월드 클리어 — 엔딩 씬 처리 필요"); // TODO: 엔딩 씬 연결
            return;
        }

        currentStageIndex = nextWorldIndex * 10;                         // 다음 월드 첫 스테이지 인덱스
        SceneManager.LoadScene(_worldSceneNames[nextWorldIndex]);         // 다음 월드 씬 로드
    }

    #endregion
}
