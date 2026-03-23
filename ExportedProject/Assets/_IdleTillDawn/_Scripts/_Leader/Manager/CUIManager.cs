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
    [SerializeField] private GameObject _deathPanel; // 플레이어 사망 시 표시할 패널

    [Header("클리어 UI")]
    [SerializeField] private GameObject _clearPanel; // 스테이지 클리어 시 표시할 패널

    [Header("스테이지 매니저 연결")]
    [SerializeField] private CStageManager _stageManager; // 이벤트 구독 대상

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
            CGameManager.Instance.OnStageIndexChanged -= UpdateStageInfoFromData;
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

        // 보스 버튼 클릭 이벤트를 StageManager에 연결
        _bossChallengeButton.onClick.AddListener(_stageManager.OnBossChallengeButtonPressed);
    }

    /// <summary>
    /// UI 초기 상태를 설정한다
    /// 보스 버튼 비활성화, 사망/클리어 패널 숨김을 기본값으로 설정한다
    /// </summary>
    private void InitializeUI()
    {
        _bossChallengeButton.gameObject.SetActive(false); // 보스 버튼 초기 숨김
        _deathPanel.SetActive(false);                     // 사망 패널 초기 숨김
        _clearPanel.SetActive(false);                     // 클리어 패널 초기 숨김
        // 씬 시작 시 게이지를 0으로 즉시 초기화 (Lerp 연출 없이 깔끔하게 시작)
        _killGaugeBar.SetValueImmediate(0, CGameManager.Instance.CurrentStageData._killGoal);
    }

    /// <summary>
    /// 현재 스테이지 번호를 UI에 표시한다
    /// 씬 시작 시 Start에서 1회 호출하여 초기 스테이지 정보를 표시한다
    /// </summary>
    private void UpdateStageInfo()
    {
        CStageData data = CGameManager.Instance.CurrentStageData;
        _stageInfoText.text = $"World {data._world} - Stage {data._stage}"; // 스테이지 번호 텍스트
    }

    /// <summary>
    /// CGameManager.OnStageIndexChanged 이벤트를 수신하여 스테이지 텍스트를 즉시 갱신한다
    /// 인덱스 증가 직후 씬 리로드 이전에 호출되므로 클리어 화면에서 다음 스테이지 번호가 즉시 반영된다
    /// 씬 리로드 후 Start에서 UpdateStageInfo가 다시 호출되므로 이중 보호 구조를 갖는다
    /// </summary>
    /// <param name="newStageData">증가된 인덱스 기준의 새 스테이지 데이터</param>
    private void UpdateStageInfoFromData(CStageData newStageData) =>
        _stageInfoText.text = $"World {newStageData._world} - Stage {newStageData._stage}"; // 즉시 반영

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

    /// <summary>
    /// 스테이지 클리어 패널을 표시한다
    /// 씬 전환 전 짧은 시간 동안 클리어 연출을 보여주는 용도로 사용한다
    /// </summary>
    private void ShowClearPanel() =>
        _clearPanel.SetActive(true); // 클리어 연출 패널 표시

    /// <summary>
    /// 플레이어 사망 패널을 표시한다
    /// 씬 리로드 전까지 사망 UI가 유지되어 플레이어가 상황을 인지할 수 있도록 한다
    /// </summary>
    private void ShowDeathPanel() =>
        _deathPanel.SetActive(true); // 사망 UI 표시

    #endregion
}
