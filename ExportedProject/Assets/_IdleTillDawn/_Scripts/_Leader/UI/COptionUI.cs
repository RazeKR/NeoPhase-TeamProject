using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 옵션 UI 컨트롤러 — 어느 씬에서도 재사용 가능한 독립 컴포넌트
///
/// [메인 메뉴]
///   옵션 버튼 클릭 → 메인메뉴 CanvasGroup 페이드 아웃 + 옵션 패널 페이드 인
///   Back 버튼      → 옵션 패널 페이드 아웃 + 메인메뉴 CanvasGroup 페이드 인
///
/// [인게임]
///   ESC 키 → Show() 자동 호출 (Update에서 처리)
///   Back 버튼 → Hide()
///
/// [텍스트 연결 방식]
///   각 버튼 GameObject 자식에 레거시 Text 컴포넌트를 달아두면
///   Awake에서 자동으로 가져옴 — 인스펙터에서 텍스트를 따로 연결할 필요 없음
/// </summary>
public class COptionUI : MonoBehaviour
{
    #region Inspector

    [Header("패널 루트 (Show/Hide 대상)")]
    [Tooltip("옵션 UI 전체를 감싸는 루트 GameObject — Option_Panel 연결")]
    [SerializeField] private GameObject _optionPanel;

    [Header("메인메뉴 페이드 대상 (옵션 열릴 때 숨길 CanvasGroup 배열)")]
    [Tooltip("GameStart_Text, Option_Text, GameQuit_Text 등 숨길 오브젝트의 CanvasGroup 배열")]
    [SerializeField] private CanvasGroup[] _mainMenuCGs;

    [Header("페이드 설정")]
    [Tooltip("페이드 인/아웃에 걸리는 시간 (초)")]
    [SerializeField] private float _fadeDuration = 0.4f;

    [Header("해상도 행 (창모드일 때만 표시)")]
    [Tooltip("해상도 버튼을 감싸는 GameObject — 전체화면일 때 숨김")]
    [SerializeField] private GameObject _resolutionRow;

    [Header("옵션 패널 열기 버튼 (메인메뉴에 있는 '옵션' 버튼)")]
    [Tooltip("클릭 시 옵션 패널을 열어주는 버튼 — 인게임 모드에서는 ESC로 열리므로 비워둬도 됨")]
    [SerializeField] private Button _openButton;

    [Header("옵션 패널 내부 버튼 (각 버튼 자식에 레거시 Text 컴포넌트를 달아두세요)")]
    [SerializeField] private Button _musicVolumeButton;
    [SerializeField] private Button _sfxVolumeButton;
    [SerializeField] private Button _fullscreenButton;
    [SerializeField] private Button _resolutionButton;
    [SerializeField] private Button _backButton;

    [Header("모드 설정")]
    [Tooltip("true = 인게임 (ESC로 열림 / 닫힘). false = 메인메뉴 (페이드 연출 적용)")]
    [SerializeField] private bool _isInGameMode = false;

    #endregion

    #region Private

    private System.Action _onBackAction;

    private CanvasGroup _optionPanelCG; // Option_Panel 의 CanvasGroup (코드로 자동 추가)
    private Coroutine   _fadeCoroutine;

    // 버튼 GameObject 자식에서 자동으로 가져오는 레거시 Text 참조
    private Text _musicVolumeText;
    private Text _sfxVolumeText;
    private Text _fullscreenText;
    private Text _resolutionText;

    #endregion

    #region Unity

    private void Awake()
    {
        // Option_Panel 에 CanvasGroup 이 없으면 자동으로 추가
        if (_optionPanel != null)
        {
            _optionPanelCG = _optionPanel.GetComponent<CanvasGroup>();
            if (_optionPanelCG == null)
                _optionPanelCG = _optionPanel.AddComponent<CanvasGroup>();

            // 시작 시 옵션 패널 숨김
            _optionPanel.SetActive(false);
            _optionPanelCG.alpha = 0f;
        }

        // 버튼 자식의 레거시 Text 자동 취득
        if (_musicVolumeButton != null) _musicVolumeText = _musicVolumeButton.GetComponentInChildren<Text>();
        if (_sfxVolumeButton   != null) _sfxVolumeText   = _sfxVolumeButton.GetComponentInChildren<Text>();
        if (_fullscreenButton  != null) _fullscreenText  = _fullscreenButton.GetComponentInChildren<Text>();
        if (_resolutionButton  != null) _resolutionText  = _resolutionButton.GetComponentInChildren<Text>();

        _openButton?.onClick.AddListener(Show);
        _musicVolumeButton?.onClick.AddListener(OnClickMusicVolume);
        _sfxVolumeButton?.onClick.AddListener(OnClickSFXVolume);
        _fullscreenButton?.onClick.AddListener(OnClickFullscreen);
        _resolutionButton?.onClick.AddListener(OnClickResolution);
        _backButton?.onClick.AddListener(OnClickBack);
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
        if (_isInGameMode && Input.GetKeyDown(KeyCode.Escape))
        {
            if (_optionPanel.activeSelf) Hide();
            else                         Show();
        }
    }

    #endregion

    #region Public API

    /// <summary>옵션 패널 열기 — 메인메뉴 페이드 아웃 + 옵션 패널 페이드 인</summary>
    public void Show()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        RefreshAll();

        if (_isInGameMode)
        {
            // 인게임은 페이드 없이 즉시 표시
            _optionPanel.SetActive(true);
            if (_optionPanelCG != null) _optionPanelCG.alpha = 1f;
        }
        else
        {
            _fadeCoroutine = StartCoroutine(Co_ShowWithFade());
        }
    }

    /// <summary>옵션 패널 닫기 — 옵션 패널 페이드 아웃 + 메인메뉴 페이드 인</summary>
    public void Hide()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        if (_isInGameMode)
        {
            _optionPanel.SetActive(false);
            if (_optionPanelCG != null) _optionPanelCG.alpha = 0f;
        }
        else
        {
            _fadeCoroutine = StartCoroutine(Co_HideWithFade());
        }
    }

    /// <summary>
    /// 메인메뉴에서 Back 버튼 동작을 외부에서 주입
    /// </summary>
    public void SetBackAction(System.Action action) => _onBackAction = action;

    #endregion

    #region Fade Coroutines

    private IEnumerator Co_ShowWithFade()
    {
        // 메인메뉴 버튼 페이드 아웃
        yield return Co_FadeMainMenu(1f, 0f);

        // 메인메뉴 버튼 입력 차단
        SetMainMenuInteractable(false);

        // 옵션 패널 활성화 후 페이드 인
        _optionPanel.SetActive(true);
        yield return Co_FadePanel(0f, 1f);
    }

    private IEnumerator Co_HideWithFade()
    {
        // 옵션 패널 페이드 아웃
        yield return Co_FadePanel(1f, 0f);
        _optionPanel.SetActive(false);

        // 메인메뉴 버튼 페이드 인
        SetMainMenuInteractable(true);
        yield return Co_FadeMainMenu(0f, 1f);

        _onBackAction?.Invoke();
    }

    private IEnumerator Co_FadePanel(float from, float to)
    {
        if (_optionPanelCG == null) yield break;

        _optionPanelCG.alpha = from;
        float t = 0f;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            _optionPanelCG.alpha = Mathf.Lerp(from, to, t / _fadeDuration);
            yield return null;
        }
        _optionPanelCG.alpha = to;
    }

    private IEnumerator Co_FadeMainMenu(float from, float to)
    {
        if (_mainMenuCGs == null || _mainMenuCGs.Length == 0) yield break;

        float t = 0f;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / _fadeDuration);
            foreach (CanvasGroup cg in _mainMenuCGs)
                if (cg != null) cg.alpha = alpha;
            yield return null;
        }

        foreach (CanvasGroup cg in _mainMenuCGs)
            if (cg != null) cg.alpha = to;
    }

    private void SetMainMenuInteractable(bool interactable)
    {
        foreach (CanvasGroup cg in _mainMenuCGs)
        {
            if (cg == null) continue;
            cg.interactable    = interactable;
            cg.blocksRaycasts  = interactable;
        }
    }

    #endregion

    #region Button Handlers

    private void OnClickMusicVolume() => CSettingsManager.Instance?.CycleMusicVolume();
    private void OnClickSFXVolume()   => CSettingsManager.Instance?.CycleSFXVolume();
    private void OnClickFullscreen()  => CSettingsManager.Instance?.ToggleFullscreen();
    private void OnClickResolution()  => CSettingsManager.Instance?.CycleResolution();

    private void OnClickBack()
    {
        if (_isInGameMode)
        {
            Hide();
        }
        else
        {
            Hide(); // Co_HideWithFade 안에서 _onBackAction 호출
        }
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
    {
        if (_musicVolumeText != null)
            _musicVolumeText.text = $"음악 볼륨 : {percent}%";
    }

    private void RefreshSFXText(int percent)
    {
        if (_sfxVolumeText != null)
            _sfxVolumeText.text = $"SFX 볼륨 : {percent}%";
    }

    private void RefreshFullscreenUI(bool isFullscreen)
    {
        if (_fullscreenText != null)
            _fullscreenText.text = $"전체화면 모드 : {(isFullscreen ? "켜기" : "끄기")}";

        if (_resolutionRow != null)
            _resolutionRow.SetActive(!isFullscreen);
    }

    private void RefreshResolutionText(int index)
    {
        if (_resolutionText != null)
            _resolutionText.text = $"해상도 : {CSettingsManager.ResolutionLabel(index)}";
    }

    #endregion
}
