using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 옵션 UI 컨트롤러 — enum 기반 상태 머신 + 단일 MainMenuRoot 관리
///
/// ══════════════════════════════════════════════════════════════
/// [Unity 계층 구조 권장]
///
///   MainMenuCanvas  (Canvas, Sort Order 0)
///   └── MainMenuRoot  ← _mainMenuRoot 연결 (CanvasGroup 자동 추가)
///       ├── GameStart_Panel
///       ├── Option_Panel
///       └── GameQuit_Panel
///
///   OptionCanvas  (Canvas, Sort Order 10) ← 독립 레이어
///   └── OptionRoot  ← _optionPanel 연결 (CanvasGroup 자동 추가)
///       ├── [볼륨/해상도/전체화면 버튼]
///       └── [Back 버튼]
///
/// ══════════════════════════════════════════════════════════════
/// [UI 상태 전환]
///
///   MainMenu ──[옵션 버튼]──→ Option
///   Option   ──[Back 버튼]──→ MainMenu
///   InGame   ──[ESC]────────→ InGameOption  (timeScale = 0)
///   InGameOption ──[ESC/Back]→ InGame       (timeScale 복구)
///
/// ══════════════════════════════════════════════════════════════
/// [페이드 처리]
///   - Time.unscaledDeltaTime 사용 → timeScale=0 상태에서도 동작
///   - 페이드 중 interactable/blocksRaycasts 차단 → 입력 충돌 방지
///   - 페이드 완료 후에만 interactable 복구
/// </summary>
public class COptionUI : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    // UI 상태 열거형
    // ──────────────────────────────────────────────────────────
    public enum UIState
    {
        MainMenu,       // 메인메뉴 활성 (옵션 닫힘)
        Option,         // 메인메뉴 → 옵션 열림
        InGame,         // 인게임 (옵션 닫힘)
        InGameOption,   // 인게임 → 옵션 열림 (timeScale = 0)
    }

    #region Inspector

    [Header("씬 초기 상태 — 메인메뉴 씬: MainMenu / 게임 씬: InGame")]
    [SerializeField] private UIState _initialState = UIState.MainMenu;

    [Header("메인메뉴 루트 (모든 메인메뉴 요소를 하나로 묶은 부모 GameObject)")]
    [Tooltip("이 오브젝트 하나의 CanvasGroup으로 메인메뉴 전체 입력/페이드 통합 제어.\n" +
             "CanvasGroup이 없으면 Awake에서 자동 추가됩니다.")]
    [SerializeField] private GameObject _mainMenuRoot;

    [Header("옵션 패널 (독립 Canvas 위에 배치, Sort Order 높게 설정 권장)")]
    [Tooltip("Option UI 루트 GameObject — CanvasGroup 없으면 자동 추가")]
    [SerializeField] private GameObject _optionPanel;

    [Header("페이드 시간 (초)")]
    [SerializeField] private float _fadeDuration = 0.3f;

    [Header("해상도 행 (전체화면 시 비활성화 표시)")]
    [Tooltip("해상도 버튼을 감싸는 부모 GameObject — CanvasGroup 없으면 자동 추가")]
    [SerializeField] private GameObject _resolutionRow;

    [Range(0.1f, 0.6f)]
    [Tooltip("전체화면 ON 시 해상도 행의 alpha 값 (어두운 정도)")]
    [SerializeField] private float _disabledAlpha = 0.35f;

    [Header("버튼 참조")]
    [Tooltip("메인메뉴 화면의 '옵션' 버튼 — 인게임 모드면 비워둬도 됨 (ESC로 열림)")]
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _musicVolumeButton;
    [SerializeField] private Button _sfxVolumeButton;
    [SerializeField] private Button _fullscreenButton;
    [SerializeField] private Button _resolutionButton;
    [SerializeField] private Button _backButton;

    #endregion

    #region Private

    private UIState _state;
    private float   _prevTimeScale = 1f;

    private CanvasGroup _mainMenuCG;
    private CanvasGroup _optionCG;
    private CanvasGroup _resolutionRowCG;

    private Coroutine     _fadeCoroutine;
    private System.Action _onBackAction;

    #endregion

    #region Properties

    public UIState CurrentState  => _state;
    public bool    IsOptionOpen  => _state == UIState.Option || _state == UIState.InGameOption;

    #endregion

    #region Unity

    private void Awake()
    {
        _state = _initialState;

        // MainMenuRoot CanvasGroup
        if (_mainMenuRoot != null)
        {
            _mainMenuCG = _mainMenuRoot.GetComponent<CanvasGroup>();
            if (_mainMenuCG == null) _mainMenuCG = _mainMenuRoot.AddComponent<CanvasGroup>();
            // 초기 상태: 메인메뉴는 완전 표시
            _mainMenuCG.alpha          = 1f;
            _mainMenuCG.interactable   = true;
            _mainMenuCG.blocksRaycasts = true;
        }

        // OptionPanel CanvasGroup — 시작 시 숨김
        if (_optionPanel != null)
        {
            _optionCG = _optionPanel.GetComponent<CanvasGroup>();
            if (_optionCG == null) _optionCG = _optionPanel.AddComponent<CanvasGroup>();
            _optionPanel.SetActive(false);
            _optionCG.alpha          = 0f;
            _optionCG.interactable   = false;
            _optionCG.blocksRaycasts = false;
        }

        // 해상도 행 CanvasGroup — 항상 활성화, alpha/interactable로만 제어
        if (_resolutionRow != null)
        {
            _resolutionRow.SetActive(true);
            _resolutionRowCG = _resolutionRow.GetComponent<CanvasGroup>();
            if (_resolutionRowCG == null) _resolutionRowCG = _resolutionRow.AddComponent<CanvasGroup>();
        }

        // 버튼 이벤트 등록
        _openButton?.onClick.AddListener(Show);
        _musicVolumeButton?.onClick.AddListener(() => CSettingsManager.Instance?.CycleMusicVolume());
        _sfxVolumeButton?.onClick.AddListener(() => CSettingsManager.Instance?.CycleSFXVolume());
        _fullscreenButton?.onClick.AddListener(() => CSettingsManager.Instance?.ToggleFullscreen());
        _resolutionButton?.onClick.AddListener(OnClickResolution);
        _backButton?.onClick.AddListener(Hide);
    }

    private void OnEnable()
    {
        if (CSettingsManager.Instance == null) return;

        CSettingsManager.Instance.OnMusicVolumeChanged += RefreshMusicText;
        CSettingsManager.Instance.OnSFXVolumeChanged   += RefreshSFXText;
        CSettingsManager.Instance.OnFullscreenChanged  += RefreshFullscreenUI;
        CSettingsManager.Instance.OnResolutionChanged  += RefreshResolutionText;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (CSettingsManager.Instance == null) return;

        CSettingsManager.Instance.OnMusicVolumeChanged -= RefreshMusicText;
        CSettingsManager.Instance.OnSFXVolumeChanged   -= RefreshSFXText;
        CSettingsManager.Instance.OnFullscreenChanged  -= RefreshFullscreenUI;
        CSettingsManager.Instance.OnResolutionChanged  -= RefreshResolutionText;
    }

    private void Update()
    {
        // 인게임 모드에서만 ESC 처리
        bool isInGame = _state == UIState.InGame || _state == UIState.InGameOption;
        if (isInGame && Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsOptionOpen) Hide();
            else               Show();
        }
    }

    #endregion

    #region Public API

    /// <summary>옵션 패널 열기</summary>
    public void Show()
    {
        if (IsOptionOpen) return;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        RefreshAll();

        switch (_state)
        {
            case UIState.MainMenu:
                _state = UIState.Option;
                _fadeCoroutine = StartCoroutine(Co_FadeToOption());
                break;

            case UIState.InGame:
                // 인게임: 시간 정지 후 즉시 표시
                _prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _state         = UIState.InGameOption;
                _fadeCoroutine = StartCoroutine(Co_ShowOptionInstant());
                break;
        }
    }

    /// <summary>옵션 패널 닫기</summary>
    public void Hide()
    {
        if (!IsOptionOpen) return;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        switch (_state)
        {
            case UIState.Option:
                _state = UIState.MainMenu;
                _fadeCoroutine = StartCoroutine(Co_FadeToMainMenu());
                break;

            case UIState.InGameOption:
                _state = UIState.InGame;
                _fadeCoroutine = StartCoroutine(Co_HideOptionInstant());
                break;
        }
    }

    /// <summary>Back 버튼 완료 후 호출할 콜백 등록 (메인메뉴 전용)</summary>
    public void SetBackAction(System.Action action) => _onBackAction = action;

    /// <summary>씬 전환 시 상태 외부에서 설정 (예: 게임 씬 로드 완료 후 InGame으로 전환)</summary>
    public void SetState(UIState newState) => _state = newState;

    #endregion

    #region Coroutines

    // 메인메뉴 → 옵션
    private IEnumerator Co_FadeToOption()
    {
        // 1. 메인메뉴 루트 페이드 아웃 + 입력 즉시 차단
        yield return Co_Fade(_mainMenuCG, 1f, 0f, false);

        // 2. 옵션 패널 활성화 후 페이드 인
        _optionPanel.SetActive(true);
        yield return Co_Fade(_optionCG, 0f, 1f, true);
    }

    // 옵션 → 메인메뉴
    private IEnumerator Co_FadeToMainMenu()
    {
        // 1. 옵션 패널 페이드 아웃 + 입력 차단
        yield return Co_Fade(_optionCG, 1f, 0f, false);
        _optionPanel.SetActive(false);

        // 2. 메인메뉴 루트 페이드 인 + 입력 복구
        yield return Co_Fade(_mainMenuCG, 0f, 1f, true);

        _onBackAction?.Invoke();
    }

    // 인게임 옵션: 즉시 표시 (timeScale=0이므로 페이드 없음)
    private IEnumerator Co_ShowOptionInstant()
    {
        _optionPanel.SetActive(true);
        if (_optionCG != null)
        {
            _optionCG.alpha          = 1f;
            _optionCG.interactable   = true;
            _optionCG.blocksRaycasts = true;
        }
        yield break;
    }

    // 인게임 옵션: 즉시 닫기 + timeScale 복구
    private IEnumerator Co_HideOptionInstant()
    {
        if (_optionCG != null)
        {
            _optionCG.alpha          = 0f;
            _optionCG.interactable   = false;
            _optionCG.blocksRaycasts = false;
        }
        _optionPanel.SetActive(false);
        Time.timeScale = _prevTimeScale;
        yield break;
    }

    /// <summary>
    /// CanvasGroup 페이드 코루틴
    /// - 페이드 시작 즉시 입력 차단 (interactable/blocksRaycasts = false)
    /// - 페이드 완료 후 endInteractable 값으로 입력 상태 설정
    /// - Time.unscaledDeltaTime 사용 → timeScale = 0 상태에서도 동작
    /// </summary>
    private IEnumerator Co_Fade(CanvasGroup cg, float from, float to, bool endInteractable)
    {
        if (cg == null) yield break;

        // 페이드 시작: 입력 즉시 차단
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        cg.alpha          = from;

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed  += Time.unscaledDeltaTime;
            cg.alpha  = Mathf.Lerp(from, to, elapsed / _fadeDuration);
            yield return null;
        }

        cg.alpha = to;

        // 페이드 완료: 최종 입력 상태 반영
        cg.interactable   = endInteractable;
        cg.blocksRaycasts = endInteractable;
    }

    #endregion

    #region Button Handlers

    private void OnClickResolution()
    {
        if (CSettingsManager.Instance == null) return;
        // 전체화면 모드에서는 클릭 무시 (_resolutionRowCG.blocksRaycasts=false로 원래 막히지만 이중 방어)
        if (!CSettingsManager.Instance.IsFullscreen)
            CSettingsManager.Instance.CycleResolution();
    }

    #endregion

    #region Refresh

    private void RefreshAll()
    {
        if (CSettingsManager.Instance == null) return;

        RefreshMusicText(CSettingsManager.Instance.MusicVolume);
        RefreshSFXText(CSettingsManager.Instance.SFXVolume);
        RefreshFullscreenUI(CSettingsManager.Instance.IsFullscreen);
        RefreshResolutionText(CSettingsManager.Instance.ResolutionIndex);
    }

    private void RefreshMusicText(int percent)
        => SetButtonLabel(_musicVolumeButton, $"음악 볼륨 : {percent}%");

    private void RefreshSFXText(int percent)
        => SetButtonLabel(_sfxVolumeButton, $"SFX 볼륨 : {percent}%");

    private void RefreshFullscreenUI(bool isFullscreen)
    {
        SetButtonLabel(_fullscreenButton, $"전체화면 모드 : {(isFullscreen ? "켜기" : "끄기")}");

        if (_resolutionRowCG != null)
        {
            _resolutionRowCG.alpha          = isFullscreen ? _disabledAlpha : 1f;
            _resolutionRowCG.interactable   = !isFullscreen;
            _resolutionRowCG.blocksRaycasts = !isFullscreen;
        }

        if (isFullscreen)
            SetButtonLabel(_resolutionButton, "해상도 : -");
        else
            RefreshResolutionText(CSettingsManager.Instance?.ResolutionIndex ?? 3);
    }

    private void RefreshResolutionText(int index)
    {
        if (CSettingsManager.Instance != null && CSettingsManager.Instance.IsFullscreen)
            return; // 전체화면 중 "-" 유지

        SetButtonLabel(_resolutionButton, $"해상도 : {CSettingsManager.ResolutionLabel(index)}");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// 버튼 자식 텍스트 설정 — TextMeshProUGUI 우선, 레거시 Text 폴백
    /// 비활성 자식 포함 검색 (true)
    /// </summary>
    private static void SetButtonLabel(Button btn, string label)
    {
        if (btn == null) return;

        var tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (tmp != null) { tmp.text = label; return; }

        var text = btn.GetComponentInChildren<Text>(true);
        if (text != null) text.text = label;
    }

    #endregion
}
