using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sliced 타입 Image의 RectTransform 너비를 조절하여 게이지를 표현하는 컴포넌트
/// fillAmount 방식은 이미지가 잘리는 구조라 Sliced 9-patch 테두리가 찌그러지는 문제가 있다
/// 대신 Fill의 sizeDelta.x를 부모 너비 × 비율로 Lerp하여 Sliced 테두리를 유지하면서 확장한다
/// Update를 사용하지 않고 이벤트 호출 시에만 Coroutine이 실행되어 성능 낭비가 없다
/// </summary>
public class CGaugeBar : MonoBehaviour
{
    #region Inspector Variables

    [Header("게이지 구성 요소")]
    [SerializeField] private RectTransform _fillRect;  // Fill 오브젝트의 RectTransform (Anchor Left, Pivot Left)
    [SerializeField] private TMP_Text      _labelText; // 게이지 중앙 오버레이 텍스트

    [Header("보간 설정")]
    [SerializeField] private float _lerpSpeed = 6f; // 너비 보간 속도 — 값이 클수록 빠르게 차오름

    #endregion

    #region Private Variables

    private RectTransform _parentRect;   // 부모 RectTransform — 전체 너비 기준값 계산에 사용
    private float         _targetWidth;  // 보간 목표 너비 (픽셀)
    private Coroutine     _lerpCoroutine; // 현재 실행 중인 Lerp 코루틴 핸들 (중복 실행 방지)

    #endregion

    #region Unity Methods

    /// <summary>
    /// 부모 RectTransform을 캐싱한다
    /// 부모 너비는 rect.width로 읽으며 Canvas Scaler 해상도에 자동으로 대응된다
    /// </summary>
    private void Awake() =>
        _parentRect = _fillRect.parent as RectTransform; // 부모 RectTransform 캐싱

    #endregion

    #region Public Methods

    /// <summary>
    /// 게이지 값을 갱신한다
    /// current / goal 비율로 목표 너비를 계산하고 Coroutine Lerp 연출을 시작한다
    /// 기존 Lerp 코루틴이 실행 중이면 중단 후 새 목표값으로 재시작하여 연속 갱신에 대응한다
    /// </summary>
    /// <param name="current">현재 수치 (예: 현재 킬카운트)</param>
    /// <param name="goal">목표 수치 (예: 목표 킬카운트)</param>
    /// <param name="labelFormat">텍스트 포맷 — null이면 current만 표시</param>
    public void SetValue(int current, int goal, string labelFormat = null)
    {
        if (goal <= 0) return; // 목표 0 이하이면 나눗셈 오류 방지

        float ratio  = Mathf.Clamp01((float)current / goal);    // 0~1 비율 계산
        _targetWidth = _parentRect.rect.width * ratio;           // 부모 너비 × 비율 = 목표 픽셀 너비

        // 텍스트는 즉시 갱신 — 숫자 정보는 Lerp 없이 바로 반영하여 정확성 보장
        if (_labelText != null)
            _labelText.text = labelFormat ?? $"{current}"; // 현재 킬수만 표시

        // 이전 Lerp 코루틴 중단 후 새 목표 너비로 재시작
        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
        _lerpCoroutine = StartCoroutine(Co_LerpWidth());
    }

    /// <summary>
    /// 게이지를 즉시 목표값으로 설정한다 (연출 없이 순간 반영)
    /// 씬 시작 초기화 또는 리셋 시 사용한다
    /// </summary>
    /// <param name="current">현재 수치</param>
    /// <param name="goal">목표 수치</param>
    public void SetValueImmediate(int current, int goal)
    {
        if (goal <= 0) return;

        if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine); // 진행 중인 연출 즉시 중단

        float ratio  = Mathf.Clamp01((float)current / goal); // 비율 계산
        _targetWidth = _parentRect.rect.width * ratio;        // 목표 픽셀 너비

        ApplyWidth(_targetWidth); // 즉시 반영 (Lerp 없음)

        if (_labelText != null) _labelText.text = $"{current}"; // 현재 킬수만 표시
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Fill RectTransform의 sizeDelta.x를 현재 값에서 _targetWidth까지 Lerp로 부드럽게 보간하는 코루틴
    /// sizeDelta.x 조작 방식은 Sliced Image의 9-patch 테두리를 유지하면서 오른쪽으로만 확장한다
    /// (Anchor와 Pivot을 왼쪽(0)으로 설정해야 왼쪽 기준으로 오른쪽이 늘어난다)
    /// 목표값과의 차이가 0.5픽셀 미만이 되면 정확한 값으로 스냅하고 종료한다
    /// </summary>
    private IEnumerator Co_LerpWidth()
    {
        if (_fillRect == null) yield break; // fillRect 미연결 시 즉시 종료

        while (Mathf.Abs(_fillRect.sizeDelta.x - _targetWidth) > 0.5f)
        {
            // 현재 너비 → 목표 너비로 매 프레임 일정 비율씩 접근
            // Time.deltaTime 기반이므로 프레임레이트에 독립적으로 동작한다
            float newWidth = Mathf.Lerp(
                _fillRect.sizeDelta.x,
                _targetWidth,
                _lerpSpeed * Time.deltaTime
            );

            ApplyWidth(newWidth); // 실제 RectTransform에 너비 적용

            yield return null; // 다음 프레임까지 대기
        }

        ApplyWidth(_targetWidth); // 오차 제거 — 정확한 최종 너비로 스냅
        _lerpCoroutine = null;    // 코루틴 핸들 초기화
    }

    /// <summary>
    /// Fill RectTransform의 sizeDelta.x만 변경하여 너비를 적용한다
    /// sizeDelta.y는 유지하여 높이 변형이 발생하지 않도록 한다
    /// </summary>
    /// <param name="width">적용할 픽셀 너비</param>
    private void ApplyWidth(float width) =>
        _fillRect.sizeDelta = new Vector2(width, _fillRect.sizeDelta.y); // x만 변경, y 유지

    #endregion
}
