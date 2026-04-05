using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 선택 패널의 버튼 하나를 담당합니다.
/// CCharacterSelectUI가 포커스/해제를 호출하며, 직접 클릭 이벤트는 CCharacterSelectUI가 구독합니다.
///
/// [계층 구조 예시]
/// ButtonRoot (Button + CCharacterSelectButton)
///  ├─ BorderImage   (Image — 테두리 스프라이트)
///  └─ CharacterRoot (Animator — Idle / Run 파라미터)
/// </summary>
public class CCharacterSelectButton : MonoBehaviour
{
    [Header("데이터")]
    [Tooltip("이 버튼에 연결할 플레이어 SO")]
    [SerializeField] private CPlayerDataSO _data;

    [Header("테두리 이미지")]
    [Tooltip("테두리 스프라이트가 붙은 Image 컴포넌트")]
    [SerializeField] private Image _borderImage;

    [Tooltip("포커스 해제 시 테두리 색상")]
    [SerializeField] private Color _normalColor   = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Tooltip("포커스 시 테두리 색상 (밝게)")]
    [SerializeField] private Color _focusedColor  = Color.white;

    [Tooltip("포커스 시 테두리 확대 배율")]
    [SerializeField] private float _focusedScale  = 1.12f;

    [Tooltip("스케일 전환 시간 (초)")]
    [SerializeField] private float _scaleDuration = 0.15f;

    [Header("캐릭터 애니메이터")]
    [Tooltip("Idle / Run 상태를 가진 Animator 컴포넌트")]
    [SerializeField] private Animator _animator;

    [Tooltip("Run 여부를 제어하는 Animator Bool 파라미터 이름")]
    [SerializeField] private string _runParam = "IsRunning";

    // ── 프로퍼티 ────────────────────────────────────────────────────────────
    public CPlayerDataSO Data => _data;

    // ── Private ─────────────────────────────────────────────────────────────
    private Vector3   _originBorderScale;
    private Coroutine _scaleCoroutine;

    private void Awake()
    {
        if (_borderImage != null)
            _originBorderScale = _borderImage.rectTransform.localScale;
    }

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>포커스 상태 전환. CCharacterSelectUI에서 호출합니다.</summary>
    public void SetFocused(bool focused)
    {
        SetBorderColor(focused ? _focusedColor : _normalColor);
        SetBorderScale(focused ? _originBorderScale * _focusedScale : _originBorderScale);
        SetAnimation(focused);
    }

    // ── Private Helpers ─────────────────────────────────────────────────────

    private void SetBorderColor(Color color)
    {
        if (_borderImage != null)
            _borderImage.color = color;
    }

    private void SetBorderScale(Vector3 target)
    {
        if (_borderImage == null) return;

        // 비활성 상태에서는 코루틴 실행 불가 → 즉시 적용
        if (!gameObject.activeInHierarchy)
        {
            _borderImage.rectTransform.localScale = target;
            return;
        }

        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(Co_ScaleBorder(target));
    }

    private void SetAnimation(bool isRunning)
    {
        if (_animator != null && _animator.isActiveAndEnabled)
            _animator.SetBool(_runParam, isRunning);
    }

    private IEnumerator Co_ScaleBorder(Vector3 target)
    {
        RectTransform rt = _borderImage.rectTransform;
        Vector3 start = rt.localScale;
        float   t     = 0f;

        while (t < _scaleDuration)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.LerpUnclamped(
                start, target, Mathf.SmoothStep(0f, 1f, t / _scaleDuration));
            yield return null;
        }

        rt.localScale = target;
    }
}
