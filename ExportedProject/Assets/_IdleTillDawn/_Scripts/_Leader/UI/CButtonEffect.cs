using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 버튼에 자동으로 부착되는 호버/클릭 효과 컴포넌트입니다.
/// CButtonManager가 씬의 모든 버튼에 자동 부착하므로 직접 추가할 필요가 없습니다.
/// - 마우스 진입 : 스케일 확대 + CButtonManager를 통한 호버 효과음 재생
/// - 마우스 이탈 : 스케일 원복
/// - 클릭        : CButtonManager를 통한 클릭 효과음 재생
/// </summary>
public class CButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    private float _hoverScale    = 1.1f;
    private float _scaleDuration = 0.12f;

    private Vector3   _originalScale;
    private Coroutine _scaleCoroutine;

    /// <summary>CButtonManager에서 호출하여 설정값을 주입합니다.</summary>
    public void Init(float hoverScale, float scaleDuration)
    {
        _hoverScale    = hoverScale;
        _scaleDuration = scaleDuration;
        _originalScale = transform.localScale;
    }

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    // ── 이벤트 ─────────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleTo(_originalScale * _hoverScale);
        CButtonManager.Instance?.PlayHoverSFX();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleTo(_originalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CButtonManager.Instance?.PlayClickSFX();
    }

    // ── 스케일 애니메이션 ───────────────────────────────────────────────────

    private void ScaleTo(Vector3 target)
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(Co_Scale(target));
    }

    private IEnumerator Co_Scale(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float   t     = 0f;

        while (t < _scaleDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(
                start, target, Mathf.SmoothStep(0f, 1f, t / _scaleDuration));
            yield return null;
        }

        transform.localScale = target;
    }
}
