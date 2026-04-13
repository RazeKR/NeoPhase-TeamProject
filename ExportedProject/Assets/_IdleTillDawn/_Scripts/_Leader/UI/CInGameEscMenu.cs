using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 ESC 키 메뉴 컨트롤러
///
/// ══════════════════════════════════════════════════════════════
/// [Unity 계층 구조 권장]
///
///   InGameCanvas
///   └── EscMenu_Root  ← 이 스크립트 부착
///       └── EscPanel  ← _escPanel 연결 (평소 비활성)
///           ├── Continue_Button  ← _continueButton
///           ├── Options_Button   ← _optionsButton
///           └── Quit_Button      ← _quitButton
///
///   OptionCanvas (기존 MainMenu와 동일 구성)
///   └── Option_Root  ← _optionUI 연결 (COptionUI 컴포넌트)
///
/// ══════════════════════════════════════════════════════════════
/// [동작]
///   - ESC 키       → ESC 패널 열기/닫기 (게임 시간 정지 없음)
///   - 계속하기 버튼 → ESC 패널 닫기
///   - 옵션 버튼     → 옵션 패널 열기 (COptionUI.Show)
///   - 종료 버튼     → 현재 시점 자동저장 후 게임 종료
///
/// ══════════════════════════════════════════════════════════════
/// [설정 주의사항]
///   - COptionUI의 Initial State: InGame
///   - COptionUI의 Handle Esc Key: false  ← 이 스크립트가 ESC 담당
///   - COptionUI의 Pause On InGame Open: false  ← 게임 정지 없음
/// </summary>
public class CInGameEscMenu : MonoBehaviour
{
    #region Inspector

    [Header("ESC 패널 (평소 비활성화 상태)")]
    [SerializeField] private GameObject _escPanel;

    [Header("옵션 UI 참조 (COptionUI가 붙은 Option_Root)")]
    [SerializeField] private COptionUI _optionUI;

    [Header("버튼 참조")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _quitButton;

    #endregion

    #region Unity

    private void Awake()
    {
        if (_escPanel != null)
            _escPanel.SetActive(false);

        _continueButton?.onClick.AddListener(OnClickContinue);
        _optionsButton?.onClick.AddListener(OnClickOptions);
        _quitButton?.onClick.AddListener(OnClickQuit);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // 옵션 패널이 열려 있으면 옵션만 먼저 닫기
        if (_optionUI != null && _optionUI.IsOptionOpen)
        {
            _optionUI.Hide();
            return;
        }

        // ESC 패널 토글
        if (_escPanel != null && _escPanel.activeSelf)
            HidePanel();
        else
            ShowPanel();
    }

    #endregion

    #region Panel Control

    private void ShowPanel()
    {
        if (_escPanel == null) return;
        _escPanel.SetActive(true);
    }

    private void HidePanel()
    {
        if (_escPanel == null) return;
        _escPanel.SetActive(false);
    }

    #endregion

    #region Button Handlers

    private void OnClickContinue()
    {
        HidePanel();
    }

    private void OnClickOptions()
    {
        _optionUI?.Show();
    }

    private void OnClickQuit()
    {
        // 현재 시점 자동저장
        CGameManager.Instance?.SaveProgress();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}
