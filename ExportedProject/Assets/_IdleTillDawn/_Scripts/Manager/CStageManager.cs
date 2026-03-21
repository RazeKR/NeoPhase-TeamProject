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

    private EStageState currentState;   // 현재 상태 (외부에서 직접 변경 불가)
    private int         currentKillCount; // 이번 스테이지 누적 처치 수
    private CStageData  stageData;       // 현재 스테이지 데이터 캐시 (GameManager에서 수신)

    #endregion

    #region Properties

    /// <summary>현재 상태를 외부에서 읽기 전용으로 노출한다</summary>
    public EStageState CurrentState => currentState; // 읽기 전용, 변경은 TransitionTo만 허용

    #endregion

    #region Unity Methods

    /// <summary>
    /// 씬 시작 시 GameManager로부터 스테이지 데이터를 받아 초기화한다
    /// 보스 매니저 이벤트를 구독하고 즉시 Farming 상태로 진입하여 스폰을 시작한다
    /// Awake가 아닌 Start를 사용하는 이유는 다른 매니저들의 Awake 초기화가 완료된 이후에 실행해야 하기 때문이다
    /// </summary>
    private void Start()
    {
        stageData = CGameManager.Instance.CurrentStageData;
        SubscribeToBossEvents();
        TransitionTo(EStageState.Farming); // 씬 시작 즉시 파밍 루프 시작
    }

    /// <summary>씬 언로드 시 이벤트 구독을 해제하여 메모리 누수를 방지한다</summary>
    private void OnDestroy()
    {
        _bossManager.OnBossDefeated  -= HandleBossDefeated;
        _bossManager.OnPlayerDefeated -= HandlePlayerDefeated;
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
        if (currentState != EStageState.Farming) return; // Farming 외 상태의 킬은 무시

        currentKillCount++;
        OnKillCountChanged?.Invoke(currentKillCount, stageData._killGoal); // UI 갱신 이벤트

        if (currentKillCount >= stageData._killGoal) TransitionTo(EStageState.BossReady);
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

    /// <summary>
    /// 보스 매니저의 결과 이벤트를 구독한다
    /// Start에서 호출하여 _bossManager가 Awake에서 초기화된 이후에 구독한다
    /// </summary>
    private void SubscribeToBossEvents()
    {
        _bossManager.OnBossDefeated   += HandleBossDefeated;
        _bossManager.OnPlayerDefeated += HandlePlayerDefeated;
    }

    /// <summary>
    /// 보스 처치 이벤트를 수신하여 StageClear 상태로 전환한다
    /// </summary>
    private void HandleBossDefeated() => TransitionTo(EStageState.StageClear);

    /// <summary>
    /// 플레이어 사망 이벤트를 수신하여 사망 UI를 띄우고 GameManager에 리스폰을 위임한다
    /// 리스폰은 씬 리로드 방식으로 처리하므로 이 클래스에서 추가 상태 복구가 필요 없다
    /// </summary>
    private void HandlePlayerDefeated()
    {
        OnPlayerDied?.Invoke();                          // 사망 UI 이벤트
        CGameManager.Instance.RespawnCurrentStage();     // 씬 리로드 위임
    }

    /// <summary>
    /// 상태 전환의 단일 진입점
    /// 이전 상태를 Exit 처리한 뒤 새 상태를 Enter 처리한다
    /// 직접 currentState를 변경하지 않고 반드시 이 메서드를 통해 전환하는 것을 강제한다
    /// </summary>
    /// <param name="nextState">전환할 목표 상태</param>
    private void TransitionTo(EStageState nextState)
    {
        ExitState(currentState);
        currentState = nextState;
        EnterState(currentState);
    }

    /// <summary>
    /// 새 상태 진입 시 각 상태별 초기화 작업을 실행한다
    /// 각 case는 해당 상태가 필요로 하는 컴포넌트만 최소한으로 조작한다
    /// </summary>
    /// <param name="state">진입할 상태</param>
    private void EnterState(EStageState state)
    {
        switch (state)
        {
            case EStageState.Farming:
                _spawnManager.StartSpawning(stageData); // 일반 몬스터 스폰 시작
                break;

            case EStageState.BossReady:
                // 스폰은 계속 유지 — Kill Goal 달성 후에도 일반 몬스터를 처치하여 성장이 지속되어야 한다
                // 스폰 중단은 플레이어가 직접 보스 도전 버튼을 눌렀을 때(BossFight 진입)에만 한다
                OnBossReady?.Invoke();                  // UI 버튼 활성화 이벤트
                break;

            case EStageState.BossFight:
                _spawnManager.StopSpawning();           // 보스 도전 시작 시에만 일반 스폰 중단
                OnBossFightStart?.Invoke();
                _bossManager.SpawnBoss(stageData);      // 보스 등장
                break;

            case EStageState.StageClear:
                OnStageClear?.Invoke();
                // 저장은 CGameManager.OnApplicationQuit에서 처리 — 여기서 호출하지 않음
                CGameManager.Instance.ProgressToNextStage();
                break;
        }
    }

    /// <summary>
    /// 상태 종료 시 해당 상태에서 할당한 리소스를 정리한다
    /// 현재는 별도 정리 로직이 없으나 이펙트, 타이머 등 확장 시 여기에 추가한다
    /// </summary>
    /// <param name="state">종료할 상태</param>
    private void ExitState(EStageState state)
    {
        // 확장 예정: 상태별 정리 로직 (이펙트 종료, 타이머 해제 등)
    }

    #endregion
}
