using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 옵션 UI 컨트롤러 — 어느 씬에서도 재사용 가능한 독립 컴포넌트
///
/// [메인 메뉴]
///   Back 버튼 → _onBackAction 콜백 호출 (CMainMenuUI 등에서 설정)
///
/// [인게임]
///   ESC 키 → Show() 자동 호출 (Update에서 처리)
///   Back 버튼 → Hide()
///
/// [설정 연동]
///   버튼 클릭 → CSettingsManager 메서드 호출
///   CSettingsManager 이벤트 구독 → 텍스트 즉시 갱신
/// </summary>
public class COptionUI : MonoBehaviour
{
    #region Inspector

    [Header("패널 루트 (Show/Hide 대상)")]
    [Tooltip("옵션 UI 전체를 감싸는 루트 GameObject")]
    [SerializeField] private GameObject _optionPanel;

    [Header("텍스트 표시")]
    [Tooltip("'음악 볼륨 : 100%' 형식으로 표시되는 TMP 텍스트")]
    [SerializeField] private TMP_Text _musicVolumeText;

    [Tooltip("'SFX 볼륨 : 100%' 형식으로 표시되는 TMP 텍스트")]
    [SerializeField] private TMP_Text _sfxVolumeText;

    [Tooltip("'전체화면 모드 : 켜기/끄기' 형식으로 표시되는 TMP 텍스트")]
    [SerializeField] private TMP_Text _fullscreenText;

    [Tooltip("'해상도 : 1920x1080' 형식으로 표시되는 TMP 텍스트")]
    [SerializeField] private TMP_Text _resolutionText;

    [Header("해상도 행 (창모드일 때만 표시)")]
    [Tooltip("해상도 텍스트 + 버튼을 감싸는 GameObject — 전체화면일 때 숨김")]
    [SerializeField] private GameObject _resolutionRow;

    [Header("버튼")]
    [SerializeField] private Button _musicVolumeButton;
    [SerializeField] private Button _sfxVolumeButton;
    [SerializeField] private Button _fullscreenButton;
    [SerializeField] private Button _resolutionButton;
    [SerializeField] private Button _backButton;

    [Header("모드 설정")]
    [Tooltip("true = 인게임 (ESC로 열림 / 닫힘). false = 메인메뉴 (Back 버튼으로만 닫힘)")]
    [SerializeField] private bool _isInGameMode = false;

    #endregion

    #region Private

    private System.Action _onBackAction; // 메인메뉴에서 Back 시 실행할 콜백

    #endregion

    #region Unity

    private void Awake()
    {
        _musicVolumeButton?.onClick.AddListener(OnClickMusicVolume);
        _sfxVolumeButton?.onClick.AddListener(OnClickSFXVolume);
        _fullscreenButton?.onClick.AddListener(OnClickFullscreen);
        _resolutionButton?.onClick.AddListener(OnClickResolution);
        _backButton?.onClick.AddListener(OnClickBack);
    }

    private void OnEnable()
    {
        if (CSettingsManager.Instance == null) return;

        CSettingsManager.Instance.OnMusicVolumeChanged  += RefreshMusicText;
        CSettingsManager.Instance.OnSFXVolumeChanged    += RefreshSFXText;
        CSettingsManager.Instance.OnFullscreenChanged   += RefreshFullscreenUI;
        CSettingsManager.Instance.OnResolutionChanged   += RefreshResolutionText;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (CSettingsManager.Instance == null) return;

        CSettingsManager.Instance.OnMusicVolumeChanged  -= RefreshMusicText;
        CSettingsManager.Instance.OnSFXVolumeChanged    -= RefreshSFXText;
        CSettingsManager.Instance.OnFullscreenChanged   -= RefreshFullscreenUI;
        CSettingsManager.Instance.OnResolutionChanged   -= RefreshResolutionText;
    }

    private void Update()
    {
        // 인게임 모드: ESC 토글
        if (_isInGameMode && Input.GetKeyDown(KeyCode.Escape))
        {
            if (_optionPanel.activeSelf) Hide();
            else                         Show();
        }
    }

    #endregion

    #region Public API

    /// <summary>옵션 패널 열기</summary>
    public void Show()
    {
        _optionPanel.SetActive(true);
        RefreshAll();
    }

    /// <summary>옵션 패널 닫기</summary>
    public void Hide() => _optionPanel.SetActive(false);

    /// <summary>
    /// 메인메뉴에서 Back 버튼 동작을 외부에서 주입
    /// 예) optionUI.SetBackAction(() => mainMenuPanel.SetActive(true));
    /// </summary>
    public void SetBackAction(System.Action action) => _onBackAction = action;

    #endregion

    #region Button Handlers

    private void OnClickMusicVolume()  => CSettingsManager.Instance?.CycleMusicVolume();
    private void OnClickSFXVolume()    => CSettingsManager.Instance?.CycleSFXVolume();
    private void OnClickFullscreen()   => CSettingsManager.Instance?.ToggleFullscreen();
    private void OnClickResolution()   => CSettingsManager.Instance?.CycleResolution();

    private void OnClickBack()
    {
        if (_isInGameMode)
        {
            Hide();
        }
        else
        {
            Hide();
            _onBackAction?.Invoke();
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

        // 해상도 행은 창모드일 때만 표시
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
