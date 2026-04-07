using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리가 가득 찼을 때 HUD에 알람 텍스트를 잠시 표시합니다.
/// CInventorySystemJ.OnInventoryFull 이벤트를 구독합니다.
///
/// [씬 설정 방법]
///   1. HUD Canvas 하위에 Text UI 오브젝트를 만들고 이 컴포넌트를 추가합니다.
///   2. Inspector의 _notificationText에 해당 Text를 연결합니다.
///   3. Text 컴포넌트에 원하는 문구를 직접 입력하고 _displayDuration, _fadeDuration을 조정합니다.
/// </summary>
public class CInventoryFullNotification : MonoBehaviour
{
    [SerializeField] private Text  _notificationText = null;
    [SerializeField] private float  _displayDuration  = 2f;   // 텍스트가 유지되는 시간(초)
    [SerializeField] private float  _fadeDuration     = 0.5f; // 페이드 인/아웃 시간(초)

    private Coroutine _showRoutine;

    private void Start()
    {
        if (CInventorySystemJ.Instance != null)
            CInventorySystemJ.Instance.OnInventoryFull += Show;

        if (_notificationText != null)
            SetAlpha(0f);
    }

    private void OnDestroy()
    {
        if (CInventorySystemJ.Instance != null)
            CInventorySystemJ.Instance.OnInventoryFull -= Show;
    }

    private void Show()
    {
        if (_notificationText == null) return;

        if (_showRoutine != null)
            StopCoroutine(_showRoutine);

        _showRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        // 페이드 인
        yield return StartCoroutine(FadeTo(1f, _fadeDuration));

        // 유지
        yield return new WaitForSeconds(_displayDuration);

        // 페이드 아웃
        yield return StartCoroutine(FadeTo(0f, _fadeDuration));
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = _notificationText.color.a;
        float elapsed    = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color c = _notificationText.color;
        c.a = alpha;
        _notificationText.color = c;
    }
}
