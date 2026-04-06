using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 패널의 열기/닫기 애니메이션을 담당합니다.
/// ShopButton의 onClick에 OpenShop을, ShopQuitButton의 onClick에 CloseShop을 연결합니다.
/// </summary>
public class CShopUI : MonoBehaviour
{
    #region Inspector Variables

    [Header("상점 패널")]
    [SerializeField] private GameObject _shopPanel = null;

    [Header("버튼")]
    [SerializeField] private Button _shopButton    = null;
    [SerializeField] private Button _shopQuitButton = null;

    [Header("애니메이션 설정")]
    [SerializeField] private float _animDuration = 0.25f;

    #endregion

    #region Private Variables

    private Coroutine _animCoroutine = null;
    private bool      _isOpen        = false;

    #endregion

    #region Unity Methods

    private void Start()
    {
        _shopPanel.SetActive(false);
        _shopPanel.transform.localScale = Vector3.zero;

        _shopButton.onClick.AddListener(OpenShop);
        _shopQuitButton.onClick.AddListener(CloseShop);
    }

    private void OnDestroy()
    {
        _shopButton.onClick.RemoveListener(OpenShop);
        _shopQuitButton.onClick.RemoveListener(CloseShop);
    }

    #endregion

    #region Public Methods

    /// <summary>상점 패널을 열립니다. 스케일 팝업 연출로 자연스럽게 나타납니다.</summary>
    public void OpenShop()
    {
        if (_isOpen) return;
        _isOpen = true;

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateScale(Vector3.zero, Vector3.one));
    }

    /// <summary>상점 패널을 닫습니다. 스케일 축소 연출로 자연스럽게 사라집니다.</summary>
    public void CloseShop()
    {
        if (!_isOpen) return;
        _isOpen = false;

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateScale(Vector3.one, Vector3.zero));
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 패널 localScale을 from → to 로 부드럽게 보간합니다.
    /// SmoothStep 커브를 사용하여 탄성감 있는 연출을 줍니다.
    /// </summary>
    private IEnumerator AnimateScale(Vector3 from, Vector3 to)
    {
        _shopPanel.SetActive(true);
        _shopPanel.transform.localScale = from;

        float elapsed = 0f;

        while (elapsed < _animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _animDuration);
            float smooth = t * t * (3f - 2f * t); // SmoothStep
            _shopPanel.transform.localScale = Vector3.LerpUnclamped(from, to, smooth);
            yield return null;
        }

        _shopPanel.transform.localScale = to;

        // 닫기 완료 시 오브젝트 비활성화
        if (to == Vector3.zero)
            _shopPanel.SetActive(false);

        _animCoroutine = null;
    }

    #endregion
}
