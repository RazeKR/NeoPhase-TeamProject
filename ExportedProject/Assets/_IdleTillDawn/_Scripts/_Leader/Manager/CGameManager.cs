using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환과 전역 게임 상태를 관리하는 매니저입니다.
/// 각 씬에 GameManager_KSH 프리팹을 배치하여 사용합니다.
/// 스테이지 SO 데이터는 CDataManager에서 조회하며 직접 보유하지 않습니다.
/// 스테이지 인덱스 추적, 씬 전환, 스케일링 공식만 담당합니다.
/// </summary>
public class CGameManager : MonoBehaviour
{
    #region Singleton

    public static CGameManager Instance { get; private set; }

    /// <summary>
    /// 씬 로드 전 가장 먼저 실행됩니다.
    /// CGameManager 인스턴스가 없으면 Resources/_Prefabs/GameManager_KSH 프리팹을 자동 생성합니다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;

        GameObject prefab = Resources.Load<GameObject>("_Prefabs/GameManager_KSH");
        if (prefab == null)
        {
            CDebug.LogWarning("[CGameManager] Resources/_Prefabs/GameManager_KSH 프리팹을 찾을 수 없습니다.");
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

    [Header("메인 메뉴 씬 이름")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu_KSH";

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

    [Header("자동 저장 설정")]
    [SerializeField] private float _autoSaveInterval = 300f;

    #endregion

    #region PrivateVariables

    private int  _currentStageIndex;  // 현재 진행 중인 스테이지 인덱스 (0-based, 씬 간 유지)
    private bool _hasEnteredGame;     // 한 번이라도 게임(스테이지)에 진입했으면 true
    private int  _selectedPlayerId = -1; // 캐릭터 선택 화면에서 선택한 플레이어 ID (-1 = 미선택)

    private CPlayerController _cachedPlayer;
    private CPlayerStatManager _cachedStatManager;

    #endregion

    #region Properties

    /// <summary>현재 진행 중인 스테이지 인덱스 (0-based).</summary>
    public int CurrentStageIndex => _currentStageIndex;

    /// <summary>캐릭터 선택 화면에서 선택한 플레이어 ID. -1이면 JSON에서 읽어야 함.</summary>
    public int SelectedPlayerId => _selectedPlayerId;

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
                CDebug.LogError("[CGameManager] CDataManager가 아직 초기화되지 않았습니다.");
                return null;
            }

            CStageDataSO data = CDataManager.Instance.GetStageByIndex(_currentStageIndex);

            // 저장된 인덱스에 해당하는 SO가 없으면 0으로 리셋 (손상된 세이브 데이터 방어)
            if (data == null && _currentStageIndex != 0)
            {
                CDebug.LogWarning($"[CGameManager] StageIndex {_currentStageIndex}에 해당하는 SO가 없습니다. " +
                                 $"StageIndex를 0으로 리셋합니다. (세이브 데이터 손상 또는 SO 미등록)");
                _currentStageIndex = 0;
                SaveProgress();
                data = CDataManager.Instance.GetStageByIndex(0);
            }

            return data;
        }
    }

    #endregion

    #region UnityMethods

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
        RedirectToCorrectWorldScene();
    }

    private void Start()
    {
        SubscribeDataManager();

        StartCoroutine(CoAutoSave());
    }

    private void SubscribeDataManager()
    {
        if (CDataManager.Instance == null) return;

        CDataManager.Instance.OnDataInitialized -= ValidateStageIndex;

        if (CDataManager.Instance.IsInitialized)
            ValidateStageIndex();
        else
            CDataManager.Instance.OnDataInitialized += ValidateStageIndex;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (CDataManager.Instance != null)
            CDataManager.Instance.OnDataInitialized -= ValidateStageIndex;
    }

    /// <summary>
    /// 로드된 StageIndex에 해당하는 SO가 실제로 존재하는지 확인합니다.
    /// SO가 없으면 0으로 리셋합니다. (세이브 데이터 손상 또는 미등록 SO 방어)
    /// </summary>
    private void ValidateStageIndex()
    {
        if (CDataManager.Instance == null) return;

        if (!CDataManager.Instance.TryGetStageByIndex(_currentStageIndex, out _))
        {
            CDebug.LogWarning($"[CGameManager] StageIndex {_currentStageIndex}에 해당하는 SO가 없어 0으로 리셋합니다.");
            _currentStageIndex = 0;
            SaveProgress();
        }
    }

    /// <summary>앱 종료 시 현재 스테이지 인덱스를 저장합니다.</summary>
    private void OnApplicationQuit() => SaveProgress();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            ResetAllData();
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

    /// <summary>
    /// 모든 플레이어 데이터(스테이지 진행도·스킬트리·인벤토리)를 초기화하고
    /// 메인 메뉴 씬으로 이동합니다. (P키)
    /// </summary>
    public void ResetAllData()
    {
        // 1. JSON 세이브 파일 삭제
        if (CJsonManager.Instance != null)  { CJsonManager.Instance.DeleteSave();  Destroy(CJsonManager.Instance.gameObject); }
        if (CAudioManager.Instance != null) { Destroy(CAudioManager.Instance.gameObject); }
        if (CDataManager.Instance != null)  { Destroy(CDataManager.Instance.gameObject); }
        if (CSettingsManager.Instance != null) { Destroy(CSettingsManager.Instance.gameObject); }
        if (CGoldManager.Instance != null)  { Destroy(CGoldManager.Instance.gameObject); }

        // 2. PlayerPrefs 전체 초기화
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 3. timeScale 복구
        Time.timeScale = 1f;

        CDebug.Log("[CGameManager] 전체 데이터 초기화 완료 → 메인 메뉴로 이동");

        // 4. 자기 자신도 파괴한 뒤 메인 메뉴 로드 → AutoCreate가 새 인스턴스를 생성
        Destroy(gameObject);
        SceneManager.LoadScene(_mainMenuSceneName);
    }

    /// <summary>
    /// 현재 스테이지 인덱스를 CJsonManager에 저장합니다.
    /// CJsonManager가 없는 타이밍이면 PlayerPrefs로 폴백합니다.
    /// </summary>
    /// <summary>
    /// 처음으로 스테이지에 진입할 때 호출합니다.
    /// playerId를 메모리에 보관하여 씬 전환 직후 CPlayerSpawner가 즉시 참조할 수 있게 합니다.
    /// </summary>
    public void MarkGameEntered(int playerId = -1)
    {
        _hasEnteredGame  = true;
        if (playerId >= 0) _selectedPlayerId = playerId;
        SaveProgress();
    }

    public void SaveProgress()
    {
        // PlayerPrefs에 모든 상태 동기화
        // (LoadProgress는 BeforeSceneLoad 타이밍에 실행 → CJsonManager 미존재 → PlayerPrefs가 유일한 저장소)
        PlayerPrefs.SetInt("StageIndex",      _currentStageIndex);
        PlayerPrefs.SetInt("HasEnteredGame",  _hasEnteredGame ? 1 : 0);
        PlayerPrefs.SetInt("SelectedPlayerId", _selectedPlayerId); // 재실행 후에도 캐릭터 선택 유지
        PlayerPrefs.Save();

        if (CJsonManager.Instance != null)
        {
            CSaveData saveData = CJsonManager.Instance.GetOrCreateSaveData();
            saveData.currentStageId = _currentStageIndex;
            if (_currentStageIndex > saveData.highestStageId)
                saveData.highestStageId = _currentStageIndex;
            // playerStatId도 항상 최신값으로 동기화 (JSON 단독 읽기 시 대비)
            if (_selectedPlayerId >= 0)
                saveData.playerStatId = _selectedPlayerId;

            if (_cachedStatManager != null && _cachedPlayer != null)
            {
                saveData.playerLevel = _cachedStatManager.CurrentLevel;
                saveData.playerExp = _cachedStatManager.CurrentExp;
                saveData.currentHp = _cachedPlayer.CurrentHealth;
                saveData.currentMana = _cachedStatManager.CurrentMana;
            }

            if (CGoldManager.Instance != null)
            {
                saveData.gold = CGoldManager.Instance.Gold;
                saveData.diamond = CGoldManager.Instance.Diamond;
            }

            CJsonManager.Instance.Save(saveData);

            if (CRankingManager.Instance != null)
            {
                CRankingManager.Instance.SaveMyRanking(saveData);
            }
        }
    }

    /// <summary>
    /// 저장된 진행 상태를 불러옵니다.
    /// BeforeSceneLoad 타이밍(CJsonManager 미존재)이면 PlayerPrefs에서 읽습니다.
    /// </summary>
    public void LoadProgress()
    {
        // PlayerPrefs는 항상 읽음 (BeforeSceneLoad 타이밍 대응)
        _currentStageIndex = PlayerPrefs.GetInt("StageIndex", 0);
        _hasEnteredGame    = PlayerPrefs.GetInt("HasEnteredGame", 0) == 1;
        _selectedPlayerId  = PlayerPrefs.GetInt("SelectedPlayerId", -1);

        // CJsonManager가 있으면 JSON으로 보완
        if (CJsonManager.Instance != null)
        {
            CSaveData saveData = CJsonManager.Instance.GetOrCreateSaveData();
            _currentStageIndex = saveData.currentStageId;

            // JSON의 playerStatId가 유효하면 우선 적용 (PlayerPrefs보다 신뢰도 높음)
            if (saveData.playerStatId >= 0 && _selectedPlayerId < 0)
                _selectedPlayerId = saveData.playerStatId;
        }
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

    public void RegisterPlayer(CPlayerController player, CPlayerStatManager statManager)
    {
        _cachedPlayer = player;
        _cachedStatManager = statManager;
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// 저장된 스테이지 인덱스에 해당하는 월드 씬이 현재 씬과 다를 경우 올바른 씬으로 이동합니다.
    /// - 최초 실행(_hasEnteredGame == false): 메인 메뉴에서 시작합니다.
    /// - 이전에 게임에 진입한 적 있음: 마지막으로 저장된 월드 씬으로 이동합니다.
    /// </summary>
    private void RedirectToCorrectWorldScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 최초 실행 — 메인 메뉴에 있지 않으면 메인 메뉴로 이동
        if (!_hasEnteredGame)
        {
            if (currentScene != _mainMenuSceneName)
            {
                CDebug.Log($"[CGameManager] 최초 실행 → 메인 메뉴 '{_mainMenuSceneName}'으로 이동");
                SceneManager.LoadScene(_mainMenuSceneName);
            }
            return;
        }

        // 이후 실행 — 마지막으로 저장된 월드 씬으로 이동
        if (_worldSceneNames == null || _worldSceneNames.Length == 0) return;

        int worldIndex = _currentStageIndex / 10;
        if (worldIndex < 0 || worldIndex >= _worldSceneNames.Length) return;

        string expectedScene = _worldSceneNames[worldIndex];
        if (string.IsNullOrEmpty(expectedScene) || currentScene == expectedScene) return;

        CDebug.Log($"[CGameManager] StageIndex={_currentStageIndex} → 월드씬 '{expectedScene}'으로 리다이렉트 (현재: '{currentScene}')");
        SceneManager.LoadScene(expectedScene);
    }

    /// <summary>
    /// 다음 월드 씬을 로드합니다.
    /// _worldSceneNames 범위를 벗어나면 엔딩 처리로 분기합니다.
    /// </summary>
    private void LoadNextWorld(int currentWorld)
    {
        int nextWorldIndex = currentWorld;

        if (nextWorldIndex >= _worldSceneNames.Length)
        {
            CDebug.Log("[CGameManager] 모든 월드 클리어 — 엔딩 씬 처리 필요");
            return;
        }

        _currentStageIndex = nextWorldIndex * 10;
        SaveProgress();
        SceneManager.LoadScene(_worldSceneNames[nextWorldIndex]);
    }

    private IEnumerator CoAutoSave()
    {
        WaitForSeconds waitTime = new WaitForSeconds(_autoSaveInterval);

        while (true)
        {
            yield return waitTime;

            // 인게임에 진입한 상태일 때만 저장 (메인 메뉴 등에서는 제외)
            if (_hasEnteredGame)
            {
                SaveProgress();
                Debug.Log($"[CGameManager] {_autoSaveInterval}초 경과 -> 자동 저장 완료");

                // (선택 사항) 화면 한쪽에 "저장 중..." 이라는 작은 UI를 잠깐 띄웠다 끄면 유저가 안심합니다!
            }
        }
    }

    #endregion
}
