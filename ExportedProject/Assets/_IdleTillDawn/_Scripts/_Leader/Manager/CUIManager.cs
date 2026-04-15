using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스테이지 진행 UI의 갱신을 전담하는 매니저
/// CStageManager의 이벤트를 구독하여 UI를 반응형으로 업데이트한다
/// UI 오브젝트 참조만 보유하며 게임 로직 판단은 절대 포함하지 않는다
/// </summary>
public class CUIManager : MonoBehaviour
{
    #region Inspector Variables

    [Header("킬 카운트 게이지")]
    [SerializeField] private CGaugeBar _killGaugeBar; // 킬카운트 진행도를 표시하는 게이지 바 컴포넌트

    [Header("보스 도전 버튼")]
    [SerializeField] private Button _bossChallengeButton; // 목표 달성 시 활성화되는 보스 도전 버튼

    [Header("스테이지 정보 UI")]
    [SerializeField] private Text _stageInfoText; // 현재 스테이지 번호 표시

    [Header("사망 UI")]
    [SerializeField] private GameObject _deathPanel;             // 플레이어 사망 시 표시할 패널
    [SerializeField] private Button     _retryButton;            // 사망 패널 - 재도전 버튼
    [SerializeField] private Button     _previousStageButton;    // 사망 패널 - 이전 스테이지 버튼
    [SerializeField] private Text       _deathKillCountText;     // 사망 패널 - 처치 수 표시 (숫자만)
    [SerializeField] private Text       _deathExpGainedText;     // 사망 패널 - 획득 경험치 표시 (숫자만)
    [SerializeField] private Text       _deathCountdownText;     // 사망 패널 - 카운트다운 표시 (숫자만)

    [Header("패널 효과음")]
    [SerializeField] private CSoundData _deathPanelSFX;  // 사망 패널 활성화 시 재생할 효과음
    [SerializeField] private CSoundData _clearPanelSFX;  // 클리어 패널 활성화 시 재생할 효과음

    [Header("클리어 UI")]
    [SerializeField] private GameObject _clearPanel;             // 스테이지 클리어 시 표시할 패널
    [SerializeField] private Button     _continueButton;         // 클리어 패널 - 계속하기 버튼
    [SerializeField] private Button     _restartButton;          // 클리어 패널 - 재시작 버튼
    [SerializeField] private Text       _clearKillCountText;     // 클리어 패널 - 처치 수 표시 (숫자만)
    [SerializeField] private Text       _clearExpGainedText;     // 클리어 패널 - 획득 경험치 표시 (숫자만)
    [SerializeField] private Text       _clearCountdownText;     // 클리어 패널 - 카운트다운 표시 (숫자만)

    [Header("스테이지 매니저 연결")]
    [SerializeField] private CStageManager _stageManager; // 이벤트 구독 대상

    #endregion

    #region Private Variables

    private const float PanelDisplayDuration = 10f; // 패널 자동 처리 대기 시간(초)

    private float _sessionExpGained;  // 이번 스테이지에서 획득한 총 경험치 (배율 적용 후)
    private Coroutine _countdownCoroutine; // 실행 중인 카운트다운 코루틴 (취소 시 사용)

    private CPlayerStatManager _subscribedStatManager; // 경험치 이벤트 구독 중인 스탯매니저

    #endregion

    #region Unity Methods

    /// <summary>
    /// 씬 시작 시 CStageManager의 이벤트를 구독하고 초기 UI 상태를 설정한다
    /// 보스 버튼은 기본적으로 비활성화 상태로 시작하여 목표 달성 전에는 클릭할 수 없도록 한다
    /// </summary>
    private void Start()
    {
        SubscribeToStageEvents();
        InitializeUI();
        UpdateStageInfo();
    }

    /// <summary>씬 언로드 시 이벤트 구독을 해제하여 메모리 누수를 방지한다</summary>
    private void OnDestroy()
    {
        _stageManager.OnKillCountChanged -= UpdateKillCount;
        _stageManager.OnBossReady        -= ShowBossChallengeButton;
        _stageManager.OnStageClear       -= ShowClearPanel;
        _stageManager.OnPlayerDied       -= ShowDeathPanel;

        // CGameManager는 DontDestroyOnLoad이므로 씬 언로드 시 반드시 구독 해제해야 누수가 없다
        if (CGameManager.Instance != null)
        {
            CGameManager.Instance.OnStageIndexChanged -= UpdateStageInfoFromData;
            CGameManager.Instance.OnPlayerRegistered  -= OnPlayerRegistered;
        }

        // 경험치 이벤트 구독 해제
        if (_subscribedStatManager != null)
            _subscribedStatManager.OnExpAdded -= AccumulateSessionExp;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// CStageManager의 모든 관련 이벤트를 구독한다
    /// 이벤트 기반으로 연결하여 UIManager가 StageManager를 직접 폴링하지 않도록 한다
    /// </summary>
    private void SubscribeToStageEvents()
    {
        _stageManager.OnKillCountChanged += UpdateKillCount;         // 킬수 변경 시 텍스트 갱신
        _stageManager.OnBossReady        += ShowBossChallengeButton; // 목표 달성 시 버튼 활성화
        _stageManager.OnStageClear       += ShowClearPanel;          // 클리어 시 패널 표시
        _stageManager.OnPlayerDied       += ShowDeathPanel;          // 사망 시 패널 표시

        // 인덱스 증가 직후 발행 — 씬 리로드 이전에 스테이지 텍스트를 즉시 갱신한다
        CGameManager.Instance.OnStageIndexChanged += UpdateStageInfoFromData;

        // 플레이어 등록 이벤트 — 경험치 추적을 위해 StatManager를 받아온다
        CGameManager.Instance.OnPlayerRegistered += OnPlayerRegistered;

        // 이미 등록된 플레이어가 있으면 즉시 연결 (Start 순서 차이 대응)
        if (CGameManager.Instance.CachedStatManager != null)
            OnPlayerRegistered(CGameManager.Instance.CachedStatManager);

        // 보스 버튼 클릭 이벤트를 StageManager에 연결
        _bossChallengeButton.onClick.AddListener(_stageManager.OnBossChallengeButtonPressed);

        // 사망 패널 버튼 연결
        if (_retryButton         != null) _retryButton.onClick.AddListener(OnRetryPressed);
        if (_previousStageButton != null) _previousStageButton.onClick.AddListener(OnPreviousStagePressed);

        // 클리어 패널 버튼 연결
        if (_continueButton != null) _continueButton.onClick.AddListener(OnContinuePressed);
        if (_restartButton  != null) _restartButton.onClick.AddListener(OnRestartPressed);
    }

    /// <summary>
    /// 플레이어가 씬에 등록될 때 호출된다.
    /// StatManager의 OnExpAdded를 구독하여 세션 경험치를 누적한다.
    /// </summary>
    private void OnPlayerRegistered(CPlayerStatManager statManager)
    {
        // 이전 구독이 있으면 해제 (중복 구독 방지)
        if (_subscribedStatManager != null)
            _subscribedStatManager.OnExpAdded -= AccumulateSessionExp;

        _subscribedStatManager = statManager;
        _subscribedStatManager.OnExpAdded += AccumulateSessionExp;
    }

    /// <summary>이번 스테이지에서 획득한 경험치를 누적한다</summary>
    private void AccumulateSessionExp(float amount) => _sessionExpGained += amount;

    /// <summary>
    /// UI 초기 상태를 설정한다
    /// 보스 버튼 비활성화, 사망/클리어 패널 숨김을 기본값으로 설정한다
    /// </summary>
    private void InitializeUI()
    {
        _bossChallengeButton.gameObject.SetActive(false); // 보스 버튼 초기 숨김
        _deathPanel.SetActive(false);                     // 사망 패널 초기 숨김
        _clearPanel.SetActive(false);                     // 클리어 패널 초기 숨김

        // 씬 시작 시 세션 통계 초기화
        _sessionExpGained = 0f;

        // 씬 시작 시 게이지를 0으로 즉시 초기화 (Lerp 연출 없이 깔끔하게 시작)
        CStageDataSO stageData = CGameManager.Instance.CurrentStageData;
        if (stageData == null)
        {
            CDebug.LogError(
                "[CUIManager] CurrentStageData가 null입니다. KillGoal을 읽을 수 없어 게이지 초기화를 건너뜁니다.\n" +
                "체크리스트:\n" +
                "  1) CDataManager가 씬에 존재하는지 확인\n" +
                "  2) CDataManager Inspector의 _stageList에 CStageDataSO가 등록됐는지 확인\n" +
                "  3) 현재 StageIndex(" + CGameManager.Instance.CurrentStageIndex +
                ")와 일치하는 StageDataSO의 StageIndex 값이 설정됐는지 확인",
                this);
            return;
        }
        _killGaugeBar.SetValueImmediate(0, stageData.KillGoal);
    }

    /// <summary>
    /// 현재 스테이지 번호를 UI에 표시한다
    /// 씬 시작 시 Start에서 1회 호출하여 초기 스테이지 정보를 표시한다
    /// </summary>
    private void UpdateStageInfo()
    {
        CStageDataSO data = CGameManager.Instance.CurrentStageData;
        if (data == null) return;
        _stageInfoText.text = $"World {data.World} - Stage {data.StageNumber}";
    }

    /// <summary>
    /// CGameManager.OnStageIndexChanged 이벤트를 수신하여 스테이지 텍스트를 즉시 갱신한다
    /// 인덱스 증가 직후 씬 리로드 이전에 호출되므로 클리어 화면에서 다음 스테이지 번호가 즉시 반영된다
    /// 씬 리로드 후 Start에서 UpdateStageInfo가 다시 호출되므로 이중 보호 구조를 갖는다
    /// </summary>
    /// <param name="newStageData">증가된 인덱스 기준의 새 스테이지 데이터</param>
    private void UpdateStageInfoFromData(CStageDataSO newStageData) =>
        _stageInfoText.text = $"World {newStageData.World} - Stage {newStageData.StageNumber}"; // 즉시 반영

    /// <summary>
    /// 킬카운트 게이지를 갱신한다
    /// CStageManager.OnKillCountChanged 이벤트에서 (현재킬, 목표킬)을 전달받아 CGaugeBar에 위임한다
    /// CGaugeBar 내부에서 fillAmount Lerp 연출과 텍스트 갱신을 처리하므로 이 메서드는 전달만 담당한다
    /// </summary>
    /// <param name="current">현재 처치 수</param>
    /// <param name="goal">목표 처치 수</param>
    private void UpdateKillCount(int current, int goal) =>
        _killGaugeBar.SetValue(current, goal); // CGaugeBar에 위임 — Lerp 연출 포함

    /// <summary>
    /// 보스 도전 버튼을 활성화한다
    /// CStageManager.OnBossReady 이벤트를 수신하여 목표 달성 시에만 노출된다
    /// </summary>
    private void ShowBossChallengeButton() =>
        _bossChallengeButton.gameObject.SetActive(true); // 목표 달성 시 버튼 노출

    // ── 사망 패널 ────────────────────────────────────────────────────────────

    /// <summary>
    /// 플레이어 사망 패널을 표시한다.
    /// 이번 스테이지 통계(킬 수, 경험치)를 표시하고 10초 카운트다운 후 자동 재도전한다.
    /// 이전 스테이지 버튼은 같은 월드 내 첫 번째 스테이지(X-1)일 때 비활성화한다.
    /// </summary>
    private void ShowDeathPanel()
    {
        _deathPanel.SetActive(true);

        // 패널 효과음 먼저 재생 → BGM 중지
        // PlayPriority: 풀·덕킹·3D 거리 감쇠를 우회하여 확실히 재생
        if (_deathPanelSFX != null)
            CAudioManager.Instance?.PlayPriority(_deathPanelSFX);
        CAudioManager.Instance?.StopBGM();

        // 이번 스테이지 통계 표시
        if (_deathKillCountText != null)
            _deathKillCountText.text = _stageManager.TotalSessionKills.ToString();
        if (_deathExpGainedText != null)
            _deathExpGainedText.text = Mathf.FloorToInt(_sessionExpGained).ToString();

        // 이전 스테이지 버튼 활성화 여부 (월드 내 첫 스테이지이면 이동 불가)
        if (_previousStageButton != null)
            _previousStageButton.interactable = CGameManager.Instance.CanGoToPreviousStage;

        // 10초 카운트다운 후 자동 재도전
        _countdownCoroutine = StartCoroutine(CountdownCoroutine(
            PanelDisplayDuration,
            _deathCountdownText,
            ExecuteRetry));
    }

    // ── 클리어 패널 ──────────────────────────────────────────────────────────

    /// <summary>
    /// 스테이지 클리어 패널을 표시한다.
    /// 이번 스테이지 통계(킬 수, 경험치)를 표시하고 10초 카운트다운 후 자동으로 계속하기 처리된다.
    /// X-10 스테이지(IsLastStage)이면 재시작 버튼을 비활성화한다.
    /// </summary>
    private void ShowClearPanel()
    {
        _clearPanel.SetActive(true);

        // 패널 효과음 먼저 재생 → BGM 중지
        // PlayPriority: 풀·덕킹·3D 거리 감쇠를 우회하여 확실히 재생
        if (_clearPanelSFX != null)
            CAudioManager.Instance?.PlayPriority(_clearPanelSFX);
        CAudioManager.Instance?.StopBGM();

        // 이번 스테이지 통계 표시
        if (_clearKillCountText != null)
            _clearKillCountText.text = _stageManager.TotalSessionKills.ToString();
        if (_clearExpGainedText != null)
            _clearExpGainedText.text = Mathf.FloorToInt(_sessionExpGained).ToString();

        // X-10 스테이지는 재시작 불가 — 반드시 다음 스테이지로 넘어가야 한다
        if (_restartButton != null)
        {
            CStageDataSO data = CGameManager.Instance.CurrentStageData;
            bool isLastStage  = data != null && data.IsLastStage;
            _restartButton.gameObject.SetActive(!isLastStage);
        }

        // 10초 카운트다운 후 자동 계속하기
        _countdownCoroutine = StartCoroutine(CountdownCoroutine(
            PanelDisplayDuration,
            _clearCountdownText,
            ExecuteContinue));
    }

    // ── 버튼 콜백 ────────────────────────────────────────────────────────────

    /// <summary>재도전 버튼 — 카운트다운을 취소하고 즉시 재도전한다</summary>
    private void OnRetryPressed()
    {
        StopCountdown();
        ExecuteRetry();
    }

    /// <summary>이전 스테이지 버튼 — 카운트다운을 취소하고 한 단계 낮은 스테이지로 이동한다</summary>
    private void OnPreviousStagePressed()
    {
        StopCountdown();
        CGameManager.Instance.GoToPreviousStage();
    }

    /// <summary>계속하기 버튼 — 카운트다운을 취소하고 즉시 다음 스테이지로 진행한다</summary>
    private void OnContinuePressed()
    {
        StopCountdown();
        ExecuteContinue();
    }

    /// <summary>
    /// 재시작 버튼 — 카운트다운을 취소하고 현재 스테이지를 처음부터 다시 시작한다.
    /// X-10 스테이지는 재시작 버튼이 숨겨지므로 이 콜백이 호출되지 않는다.
    /// </summary>
    private void OnRestartPressed()
    {
        StopCountdown();
        ExecuteRestart();
    }

    // ── 실행 로직 ─────────────────────────────────────────────────────────────

    /// <summary>재도전 실행 — 킬카운트 초기화 후 씬 리로드</summary>
    private void ExecuteRetry()
    {
        _stageManager.ResetKillCountOnDeath();
        CGameManager.Instance.RespawnCurrentStage();
    }

    /// <summary>계속하기 실행 — 다음 스테이지로 진행</summary>
    private void ExecuteContinue() =>
        CGameManager.Instance.ProgressToNextStage();

    /// <summary>재시작 실행 — 킬카운트 초기화 후 동일 씬 리로드</summary>
    private void ExecuteRestart()
    {
        _stageManager.ResetKillCountOnDeath();
        CGameManager.Instance.RespawnCurrentStage();
    }

    // ── 카운트다운 코루틴 ────────────────────────────────────────────────────

    /// <summary>실행 중인 카운트다운 코루틴을 중단한다</summary>
    private void StopCountdown()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
    }

    /// <summary>
    /// 지정된 시간 동안 카운트다운을 진행하고 완료되면 콜백을 실행하는 코루틴.
    /// countdownText가 연결된 경우 남은 초(정수)를 매 프레임 갱신한다.
    /// Time.unscaledDeltaTime을 사용하여 timeScale 영향을 받지 않는다.
    /// </summary>
    private IEnumerator CountdownCoroutine(float duration, Text countdownText, System.Action onComplete)
    {
        float remaining = duration;

        while (remaining > 0f)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(remaining).ToString();

            yield return null;
            remaining -= Time.unscaledDeltaTime;
        }

        if (countdownText != null)
            countdownText.text = "0";

        _countdownCoroutine = null;
        onComplete?.Invoke();
    }

    #endregion
}
