using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환과 전역 게임 상태를 관리하는 최상위 매니저입니다.
/// DontDestroyOnLoad로 씬이 바뀌어도 파괴되지 않습니다.
/// 스테이지 SO 데이터는 CDataManager에서 조회하며 직접 보유하지 않습니다.
/// 스테이지 인덱스 추적, 씬 전환, 스케일링 공식만 담당합니다.
/// </summary>
public class CGameManager : MonoBehaviour
{
    #region Singleton

    public static CGameManager Instance { get; private set; } // 전역 접근점, 씬 전환 후에도 유지

    /// <summary>
    /// 씬 로드 전 가장 먼저 실행됩니다.
    /// CGameManager 인스턴스가 없으면 Resources/_Prefabs/GameManager_KSH 프리팹을 자동 생성합니다.
    /// 어떤 씬에서 Play를 시작해도 Instance가 null이 되지 않습니다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;

        GameObject prefab = Resources.Load<GameObject>("_Prefabs/GameManager_KSH");
        if (prefab == null)
        {
            Debug.LogWarning("[CGameManager] Resources/_Prefabs/GameManager_KSH 프리팹을 찾을 수 없습니다.");
            new GameObject("GameManager_KSH").AddComponent<CGameManager>();
            return;
        }

        Instantiate(prefab);
    }

    #endregion

    #region Events

    /// <summary>
    /// 스테이지 인덱스가 증가한 직후 발행됩니다.
    /// 새 CStageDataSO를 인자로 전달하여 구독자가 별도 조회 없이 바로 사용할 수 있습니다.
    /// </summary>
    public event Action<CStageDataSO> OnStageIndexChanged; // (새 스테이지 데이터) — CUIManager가 구독

    #endregion

    #region InspectorVariables

    [Header("월드별 씬 이름")]
    [SerializeField] private string[] _worldSceneNames; // world[0]="World_1", world[1]="World_2"...

    [Header("스탯 스케일링 공식")]
    [SerializeField] private float _hpGrowthRate      = 0.1f;  // 스테이지당 HP 증가율 (기본 10%)
    [SerializeField] private float _bossHpGrowthRate  = 0.2f;  // 스테이지당 보스 HP 증가율 (기본 20%)
    [SerializeField] private float _bossAtkGrowthRate = 0.15f; // 스테이지당 보스 공격력 증가율 (기본 15%)

    [Header("드롭 옵션")]
    [Range(0f, 100f)][SerializeField] private float _baseDropChance = 5f;    // 기본 드롭 확률
    [Range(0f, 100f)][SerializeField] private float _maxDropChance  = 25f;   // 최대 드롭 확률
    [Range(0f, 1f)]  [SerializeField] private float _growthRate     = 0.005f; // 드롭 증가율

    #endregion

    #region PrivateVariables

    private int _currentStageIndex; // 현재 진행 중인 스테이지 인덱스 (0-based, 씬 간 유지)

    #endregion

    #region Properties

    /// <summary>현재 진행 중인 스테이지 인덱스 (0-based).</summary>
    public int CurrentStageIndex => _currentStageIndex;

    /// <summary>
    /// 현재 스테이지의 SO 데이터를 CDataManager에서 조회하여 반환합니다.
    /// CDataManager가 초기화되지 않았거나 해당 인덱스의 SO가 없으면 null을 반환합니다.
    /// </summary>
    public CStageDataSO CurrentStageData
    {
        get
        {
            if (CDataManager.Instance == null || !CDataManager.Instance.IsInitialized)
            {
                Debug.LogError("[CGameManager] CDataManager가 아직 초기화되지 않았습니다.");
                return null;
            }
            return CDataManager.Instance.GetStageByIndex(_currentStageIndex);
        }
    }

    #endregion

    #region UnityMethods

    /// <summary>씬 중복 로드 방지 및 DontDestroyOnLoad 등록.</summary>
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    /// <summary>앱 종료 시 현재 스테이지 인덱스를 저장합니다.</summary>
    private void OnApplicationQuit() => SaveProgress();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            ResetToFirstStage();
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// 보스 처치 후 다음 스테이지로 이동합니다.
    /// X-10 클리어 시 다음 월드 씬을 로드하고, 그 외에는 인덱스 증가 후 씬을 리로드합니다.
    /// </summary>
    public void ProgressToNextStage()
    {
        CStageDataSO current = CurrentStageData;
        if (current == null) return;

        if (current.IsLastStage)
        {
            LoadNextWorld(current.World);
            return;
        }

        _currentStageIndex++;
        SaveProgress();
        OnStageIndexChanged?.Invoke(CurrentStageData);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>플레이어 사망 시 동일 스테이지를 재시작합니다.</summary>
    public void RespawnCurrentStage() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    /// <summary>스테이지를 1-1로 초기화합니다 (P키 디버그용).</summary>
    public void ResetToFirstStage()
    {
        _currentStageIndex = 0;
        SaveProgress();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 현재 스테이지 인덱스를 CJsonManager에 저장합니다.
    /// CJsonManager가 없는 타이밍이면 PlayerPrefs로 폴백합니다.
    /// </summary>
    public void SaveProgress()
    {
        if (CJsonManager.Instance != null)
        {
            CSaveData saveData = CJsonManager.Instance.GetOrCreateSaveData();
            saveData.currentStageId = _currentStageIndex;
            if (_currentStageIndex > saveData.highestStageId)
                saveData.highestStageId = _currentStageIndex;
            CJsonManager.Instance.Save(saveData);
            return;
        }

        // CJsonManager 미존재 시 폴백 (RuntimeInitialize 타이밍 등)
        PlayerPrefs.SetInt("StageIndex", _currentStageIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 저장된 스테이지 인덱스를 불러옵니다.
    /// CJsonManager가 없는 타이밍이면 PlayerPrefs에서 폴백으로 읽습니다.
    /// </summary>
    public void LoadProgress()
    {
        if (CJsonManager.Instance != null)
        {
            CSaveData saveData = CJsonManager.Instance.GetOrCreateSaveData();
            _currentStageIndex = saveData.currentStageId;
            return;
        }

        // CJsonManager 미존재 시 폴백
        _currentStageIndex = PlayerPrefs.GetInt("StageIndex", 0);
    }

    /// <summary>
    /// 현재 stageIndex 기반 일반 몬스터 HP 배율을 반환합니다.
    /// 예) Stage 0: 1.0x / Stage 10: 2.0x / Stage 20: 3.0x
    /// </summary>
    public float GetEnemyHpMultiplier() => 1f + (_hpGrowthRate * _currentStageIndex);

    /// <summary>
    /// 현재 stageIndex 기반 보스 HP 배율을 반환합니다.
    /// 예) Stage 0: 1.0x / Stage 10: 3.0x / Stage 20: 5.0x
    /// </summary>
    public float GetBossHpMultiplier() => 1f + (_bossHpGrowthRate * _currentStageIndex);

    /// <summary>
    /// 현재 stageIndex 기반 보스 공격력 배율을 반환합니다.
    /// 예) Stage 0: 1.0x / Stage 10: 2.5x / Stage 20: 4.0x
    /// </summary>
    public float GetBossAtkMultiplier() => 1f + (_bossAtkGrowthRate * _currentStageIndex);

    /// <summary>현재 스테이지 인덱스에 따른 아이템 드롭 확률을 반환합니다.</summary>
    public float GetCurrentDropChance()
    {
        float failReduction = Mathf.Pow(1f - _growthRate, _currentStageIndex);
        return _maxDropChance - (_maxDropChance - _baseDropChance) * failReduction;
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// 다음 월드 씬을 로드합니다.
    /// _worldSceneNames 범위를 벗어나면 엔딩 처리로 분기합니다.
    /// </summary>
    private void LoadNextWorld(int currentWorld)
    {
        int nextWorldIndex = currentWorld;

        if (nextWorldIndex >= _worldSceneNames.Length)
        {
            Debug.Log("[CGameManager] 모든 월드 클리어 — 엔딩 씬 처리 필요");
            return;
        }

        _currentStageIndex = nextWorldIndex * 10;
        SaveProgress();
        SceneManager.LoadScene(_worldSceneNames[nextWorldIndex]);
    }

    #endregion
}
