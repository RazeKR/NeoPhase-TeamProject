using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 ESC 키 메뉴 컨트롤러
///
/// ══════════════════════════════════════════════════════════════
/// [Unity 계층 구조 권장]
///
///   InGameCanvas
///   └── EscMenu_Root  ← 이 스크립트 부착 (항상 활성 상태 유지!)
///       └── EscPanel  ← _escPanel 연결 (평소 비활성)
///           ├── Continue_Button  ← _continueButton
///           ├── Options_Button   ← _optionsButton
///           └── Quit_Button      ← _quitButton
///
///   OptionCanvas (기존 MainMenu와 동일 구성)
///   └── Option_Root  ← _optionUI 연결 (COptionUI 컴포넌트)
///
/// ══════════════════════════════════════════════════════════════
/// [ESC 키 우선순위]
///   1. 옵션 패널이 열려있으면 → 옵션 패널 닫기
///   2. ESC 패널이 열려있으면  → ESC 패널 닫기
///   3. 둘 다 닫혀있으면       → ESC 패널 열기
///
/// ══════════════════════════════════════════════════════════════
/// [COptionUI 인스펙터 설정]
///   - Initial State        : InGame
///   - Pause On InGame Open : false (게임 정지 없음)
///   ※ COptionUI.Update()는 InGame 상태에서 ESC를 처리하지 않음
///      ESC 전담은 이 스크립트(CInGameEscMenu)
///
/// ══════════════════════════════════════════════════════════════
/// [주의] EscMenu_Root는 항상 활성(active=true) 상태여야 Update가 동작함
///        EscPanel만 비활성으로 시작할 것
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
        else
            Debug.LogError("[CInGameEscMenu] _escPanel이 null입니다. Inspector에서 EscPanel을 연결하세요.");

        if (_optionUI != null)
            _optionUI.SetState(COptionUI.UIState.InGame);

        _continueButton?.onClick.AddListener(OnClickContinue);
        _optionsButton?.onClick.AddListener(OnClickOptions);
        _quitButton?.onClick.AddListener(OnClickQuit);
    }

    private void OnEnable()
    {
        StartCoroutine(CoBindInput());
    }

    private void OnDisable()
    {
        if (CInputDispatcher.Instance != null)
            CInputDispatcher.Instance.OnOption -= OnEscInput;
    }

    private System.Collections.IEnumerator CoBindInput()
    {
        while (CInputDispatcher.Instance == null) yield return null;
        CInputDispatcher.Instance.OnOption += OnEscInput;
    }

    // CInputDispatcher.OnOption 이벤트 콜백 (ESC 키)
    private void OnEscInput()
    {
        if (_escPanel == null) return;

        // 옵션 패널이 열려 있으면 옵션만 먼저 닫기
        if (_optionUI != null && _optionUI.IsOptionOpen)
        {
            _optionUI.Hide();
            return;
        }

        // ESC 패널 토글
        if (_escPanel.activeSelf)
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
