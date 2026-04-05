using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenu_KSH 씬의 버튼 동작을 처리합니다.
///
/// [주의사항]
/// - 이 컴포넌트는 항상 활성 상태인 오브젝트(예: UI_KSH)에 배치해야 합니다.
/// - _mainUICG와 _playerSelectCG는 반드시 인스펙터에서 연결해야 합니다.
/// - 씬 재로드 후에도 Start()에서 상태를 보장합니다.
/// </summary>
public class CMainMenuUI : MonoBehaviour
{
    [Header("메인 UI")]
    [Tooltip("게임시작/옵션/게임종료 버튼이 포함된 메인 UI의 CanvasGroup")]
    [SerializeField] private CanvasGroup _mainUICG;

    [Header("플레이어 선택 패널")]
    [Tooltip("PlayerSelectPanel의 CanvasGroup")]
    [SerializeField] private CanvasGroup _playerSelectCG;

    [Header("전환 설정")]
    [SerializeField] private float _fadeDuration = 0.3f;

    [Header("옵션 UI")]
    [SerializeField] private COptionUI _optionUI;

    [Header("씬 이름")]
    [SerializeField] private string _firstStageSceneName = "Stage1_KSH";

    private bool _isTransitioning = false;
    private Coroutine _activeCoroutine;

    private void Start()
    {
        // Start: 씬의 모든 Awake/OnEnable 완료 후 실행
        _isTransitioning = false;
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }

        // COptionUI가 Awake에서 _mainMenuCG를 건드릴 수 있으므로
        // Start에서 한 번 더 강제 적용
        ForceMainUIState();

        // 혹시 다른 컴포넌트의 Start()가 더 늦게 실행될 경우를 대비해
        // 1프레임 후에도 한 번 더 강제 적용
        StartCoroutine(Co_ForceStateNextFrame());
    }

    private void OnEnable()
    {
        // 씬 재로드 시 초기 상태 즉시 보장 (Start보다 먼저 실행)
        _isTransitioning = false;
    }

    private System.Collections.IEnumerator Co_ForceStateNextFrame()
    {
        yield return null; // 1프레임 대기 → 모든 Start() 완료 보장
        ForceMainUIState();

        // COptionUI가 메인메뉴 CanvasGroup을 건드렸을 수 있으므로 명시적으로 리셋
        if (_optionUI != null)
            _optionUI.SetState(COptionUI.UIState.MainMenu);
    }

    // ── 공개 API ────────────────────────────────────────────────────────────

    /// <summary>게임시작 버튼 OnClick</summary>
    public void OnClickGameStart()
    {
        if (_isTransitioning) return;
        _activeCoroutine = StartCoroutine(Co_Transition(_mainUICG, _playerSelectCG));
    }

    /// <summary>뒤로 버튼 OnClick</summary>
    public void OnClickBack()
    {
        if (_isTransitioning) return;
        _activeCoroutine = StartCoroutine(Co_Transition(_playerSelectCG, _mainUICG));
    }

    /// <summary>옵션 버튼 OnClick</summary>
    public void OnClickOption()
    {
        _optionUI?.Show();
    }

    /// <summary>게임종료 버튼 OnClick</summary>
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 캐릭터 선택 확인 버튼 OnClick (CCharacterSelectUI.OnClickEnterGame 대신 사용 가능)
    /// </summary>
    public void OnClickEnterGame()
    {
        CGameManager.Instance.MarkGameEntered();
        SceneManager.LoadScene(_firstStageSceneName);
    }

    // ── Private ─────────────────────────────────────────────────────────────

    /// <summary>메인 UI 활성 / PlayerSelect 비활성 상태를 강제로 적용합니다.</summary>
    private void ForceMainUIState()
    {
        ApplyCG(_mainUICG,      alpha: 1f, interactive: true);
        ApplyCG(_playerSelectCG, alpha: 0f, interactive: false);
    }

    private static void ApplyCG(CanvasGroup cg, float alpha, bool interactive)
    {
        if (cg == null) return;
        cg.alpha          = alpha;
        cg.interactable   = interactive;
        cg.blocksRaycasts = interactive;
    }

    private IEnumerator Co_Transition(CanvasGroup fromCG, CanvasGroup toCG)
    {
        _isTransitioning = true;

        // 페이드 아웃
        if (fromCG != null)
        {
            fromCG.interactable   = false;
            fromCG.blocksRaycasts = false;

            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                fromCG.alpha = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, t / _fadeDuration));
                yield return null;
            }
            fromCG.alpha = 0f;
        }

        // 페이드 인
        if (toCG != null)
        {
            toCG.alpha          = 0f;
            toCG.interactable   = false;
            toCG.blocksRaycasts = false;

            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                toCG.alpha = Mathf.Lerp(0f, 1f, Mathf.SmoothStep(0f, 1f, t / _fadeDuration));
                yield return null;
            }
            toCG.alpha          = 1f;
            toCG.interactable   = true;
            toCG.blocksRaycasts = true;
        }

        _isTransitioning   = false;
        _activeCoroutine   = null;
    }
}
