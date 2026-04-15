using System;
using UnityEngine;

/// <summary>
/// 스테이지 진행의 핵심 상태머신
/// 킬카운트 집계, 상태 전환 판정, 이벤트 발행을 전담한다
/// 다른 매니저들은 이 클래스의 이벤트를 구독하여 반응하고 직접 호출은 최소화한다
/// 상태 전환은 반드시 TransitionTo를 통해서만 이루어지며 외부에서 직접 상태를 변경할 수 없다
/// </summary>
public class CStageManager : MonoBehaviour
{
    #region State Definition

    /// <summary>
    /// 스테이지 진행 상태를 정의하는 열거형
    /// Idle → Farming → BossReady → BossFight → StageClear 순으로 단방향 전환된다
    /// 플레이어 사망 시 Idle로 복귀하지 않고 씬 자체를 리로드하여 상태를 초기화한다
    /// </summary>
    public enum EStageState
    {
        Idle,       // 씬 초기화 전 대기 상태
        Farming,    // 일반 몬스터 처치 루프 진행 중
        BossReady,  // 목표 킬수 달성, 보스 도전 버튼 대기 중
        BossFight,  // 보스 전투 진행 중
        StageClear  // 스테이지 클리어 완료, 다음 단계로 이동 처리 중
    }

    #endregion

    #region Events

    public event Action<int, int> OnKillCountChanged; // (현재킬수, 목표킬수) — UIManager가 구독하여 UI 갱신
    public event Action           OnBossReady;         // 목표 달성 — UIManager가 구독하여 버튼 활성화
    public event Action           OnBossFightStart;    // 보스 등장 — CSpawnManager가 구독하여 스폰 정지
    public event Action           OnStageClear;        // 클리어 — UIManager가 구독하여 클리어 연출
    public event Action           OnPlayerDied;        // 사망 — UIManager가 구독하여 사망 UI 표시

    #endregion

    #region Inspector Variables

    [Header("매니저 연결")]
    [SerializeField] private CSpawnManager _spawnManager; // 스폰 시작/정지 제어
    [SerializeField] private CBossManager  _bossManager;  // 보스 스폰 및 결과 이벤트 수신

    #endregion

    #region Private Variables

    private EStageState  currentState;      // 현재 상태 (외부에서 직접 변경 불가)
    private int          currentKillCount;  // 보스 도전 조건용 처치 수 (Farming 상태에서만 집계)
    private int          _totalSessionKills; // 이번 씬 진입 후 처치한 총 몬스터 수 (상태 무관)
    private CStageDataSO stageData;         // 현재 스테이지 데이터 캐시 (GameManager에서 수신)
    private bool         _stageCleared;     // StageClear 여부 — OnDestroy에서 중복 저장 방지용

    #endregion

    #region Properties

    /// <summary>현재 상태를 외부에서 읽기 전용으로 노출한다</summary>
    public EStageState CurrentState => currentState;

    /// <summary>보스 도전 조건용 처치 수 (Farming 상태에서만 집계)</summary>
    public int CurrentKillCount => currentKillCount;

    /// <summary>이번 씬 진입 후 상태 무관하게 처치한 총 몬스터 수 — 결과 패널 표시용</summary>
    public int TotalSessionKills => _totalSessionKills;

    #endregion

    #region Unity Methods

    /// <summary>
    /// 씬 시작 시 GameManager로부터 스테이지 데이터를 받아 초기화한다
    /// 저장된 킬카운트가 있으면 복원하여 게임 재시작 후에도 진행도가 유지된다
    /// </summary>
    private void Start()
    {
        stageData = CGameManager.Instance.CurrentStageData;

        if (stageData == null)
        {
            CDebug.LogError(
                "[CStageManager] CurrentStageData가 null입니다.\n" +
                "체크리스트:\n" +
                "  1) CDataManager가 씬에 존재하는지 확인\n" +
                "  2) CDataManager Inspector의 _stageList에 CStageDataSO가 등록됐는지 확인\n" +
                "  3) 현재 StageIndex(" + CGameManager.Instance.CurrentStageIndex +
                ")와 일치하는 StageDataSO의 StageIndex 값이 설정됐는지 확인",
                this);
            return;
        }

        bool alreadyBossReady = RestoreKillCount(); // 저장된 킬카운트 복원, 목표 달성 여부 반환
        SubscribeToBossEvents();

        if (alreadyBossReady)
        {
            // 킬 목표를 이미 달성한 채로 재시작 — 스폰은 유지하되 BossReady로 직행
            _spawnManager.StartSpawning(stageData);
            TransitionTo(EStageState.BossReady);
        }
        else
        {
            TransitionTo(EStageState.Farming);
        }
    }

    /// <summary>씬 언로드 시 킬카운트를 저장하고 이벤트 구독을 해제한다</summary>
    private void OnDestroy()
    {
        _bossManager.OnBossDefeated   -= HandleBossDefeated;
        _bossManager.OnPlayerDefeated -= HandlePlayerDefeated;

        // 스테이지 클리어 시엔 EnterState(StageClear)에서 이미 저장 완료 → 중복 저장 방지
        if (!_stageCleared)
            SaveKillCountToData();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 일반 몬스터가 처치될 때 CSpawnManager에서 호출한다
    /// Farming 상태일 때만 카운트를 집계하여 잘못된 상태의 킬이 반영되지 않도록 한다
    /// 목표 달성 시 즉시 BossReady로 전환하여 보스 도전 버튼을 활성화한다
    /// </summary>
    public void RegisterKill()
    {
        // 상태 무관 — 결과 패널 표시용 총 처치 수는 항상 누적
        _totalSessionKills++;

        if (currentState != EStageState.Farming) return;

        currentKillCount++;
        OnKillCountChanged?.Invoke(currentKillCount, stageData.KillGoal);

        // 메모리상 CSaveData만 갱신 (파일 쓰기는 씬 언로드/종료 시 한 번만)
        UpdateKillCountInMemory();

        if (currentKillCount >= stageData.KillGoal) TransitionTo(EStageState.BossReady);
    }

    /// <summary>
    /// 플레이어가 보스 도전 버튼을 눌렀을 때 UIManager에서 호출한다
    /// BossReady 상태에서만 전환을 허용하여 잘못된 입력을 원천 차단한다
    /// </summary>
    public void OnBossChallengeButtonPressed()
    {
        if (currentState != EStageState.BossReady) return;
        TransitionTo(EStageState.BossFight);
    }

    #endregion

    #region Private Methods

    private void SubscribeToBossEvents()
    {
        _bossManager.OnBossDefeated   += HandleBossDefeated;
        _bossManager.OnPlayerDefeated += HandlePlayerDefeated;
    }

    private void HandleBossDefeated() => TransitionTo(EStageState.StageClear);

    private void HandlePlayerDefeated() => OnPlayerDied?.Invoke();

    /// <summary>
    /// 보스전 이외 경로(일반 몬스터 등)에서 플레이어가 사망했을 때 외부에서 호출한다.
    /// OnPlayerDied 이벤트를 발행하여 UIManager가 사망 패널을 표시하도록 한다.
    /// 씬 리로드는 UIManager의 카운트다운 또는 재도전 버튼 클릭 시 수행된다.
    /// </summary>
    public void TriggerPlayerDeath() => OnPlayerDied?.Invoke();

    private void TransitionTo(EStageState nextState)
    {
        ExitState(currentState);
        currentState = nextState;
        EnterState(currentState);
    }

    private void EnterState(EStageState state)
    {
        switch (state)
        {
            case EStageState.Farming:
                _spawnManager.StartSpawning(stageData);
                break;

            case EStageState.BossReady:
                OnBossReady?.Invoke();
                break;

            case EStageState.BossFight:
                _spawnManager.StopSpawning();
                OnBossFightStart?.Invoke();
                _bossManager.SpawnBoss(stageData);
                break;

            case EStageState.StageClear:
                _stageCleared = true;
                SaveKillCountReset(); // 다음 스테이지를 위해 킬카운트 0으로 저장

                // ProgressToNextStage는 CUIManager가 카운트다운 후 직접 호출한다
                // (계속하기 버튼 또는 10초 타임아웃)
                OnStageClear?.Invoke();
                break;
        }
    }

    private void ExitState(EStageState state)
    {
        // 확장 예정: 상태별 정리 로직 (이펙트 종료, 타이머 해제 등)
    }

    // ── 킬카운트 저장/복원 ────────────────────────────────────────────────

    /// <summary>
    /// 씬 시작 시 저장된 킬카운트를 복원한다.
    /// 같은 스테이지이고 KillGoal 미만일 때만 복원 (다른 스테이지·클리어 후 잔류값 방어)
    /// KillGoal 이상이면 KillGoal로 복원하고 true를 반환하여 BossReady로 직접 전환할 수 있게 한다.
    /// </summary>
    /// <returns>복원된 킬카운트가 KillGoal 이상이면 true (BossReady 상태로 시작해야 함)</returns>
    private bool RestoreKillCount()
    {
        if (CJsonManager.Instance == null) return false;
        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();

        if (data.currentStageId != stageData.StageIndex) return false; // 다른 스테이지면 복원 안 함

        currentKillCount = Mathf.Clamp(data.currentKillCount, 0, stageData.KillGoal);

        if (currentKillCount > 0)
            OnKillCountChanged?.Invoke(currentKillCount, stageData.KillGoal);

        return currentKillCount >= stageData.KillGoal;
    }

    /// <summary>
    /// 현재 킬카운트를 CSaveData 메모리에만 반영한다. (파일 쓰기 없음)
    /// 실제 파일 저장은 씬 언로드(OnDestroy) 또는 앱 종료(OnApplicationQuit) 시 수행된다.
    /// </summary>
    private void UpdateKillCountInMemory()
    {
        if (CJsonManager.Instance == null) return;
        CJsonManager.Instance.GetOrCreateSaveData().currentKillCount = currentKillCount;
    }

    /// <summary>플레이어 사망 시 현재 스테이지 킬카운트를 0으로 초기화하고 UI와 파일을 갱신한다.</summary>
    public void ResetKillCountOnDeath()
    {
        currentKillCount = 0;
        OnKillCountChanged?.Invoke(currentKillCount, stageData != null ? stageData.KillGoal : 0);
        if (CJsonManager.Instance != null)
        {
            CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();
            data.currentKillCount = 0;
            CJsonManager.Instance.Save(data);
        }
    }

    /// <summary>다음 스테이지 시작을 위해 킬카운트를 0으로 초기화하고 파일에 저장한다.</summary>
    private void SaveKillCountReset()
    {
        if (CJsonManager.Instance == null) return;
        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();
        data.currentKillCount = 0;
        CJsonManager.Instance.Save(data);
    }

    /// <summary>현재 킬카운트를 파일에 저장한다. (사망/씬 언로드 시 호출)</summary>
    private void SaveKillCountToData()
    {
        if (CJsonManager.Instance == null || stageData == null) return;
        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();
        data.currentKillCount = currentKillCount;
        CJsonManager.Instance.Save(data);
    }

    #endregion
}
