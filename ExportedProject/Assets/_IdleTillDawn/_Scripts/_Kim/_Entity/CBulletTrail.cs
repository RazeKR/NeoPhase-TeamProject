using UnityEngine;

/// <summary>
/// 총알 비주얼 시스템 — CBullet과 같은 오브젝트에 추가
///
/// ──────────────────────────────────────────────────────
/// [구조]
///   모든 비주얼이 Bullet의 자식 오브젝트로 구성됨
///   → FOW, 카메라 컬링, 레이어가 Bullet과 완전히 동일하게 동작
///   → 첫 발사 투명 문제 없음
///   → 런타임 오브젝트 스폰 없음 (GC 없음)
///
/// [비주얼 구성]
///   [TrailFar: 넓고 흐린 타원] [TrailNear: 중간 타원] [Head: 밝은 작은 원]  →이동
///
/// [프리팹 구성 방법]
///   Bullet (Root)
///     ├── Rigidbody2D, Collider2D, CBullet, CBulletTrail
///     ├── TrailFar  (자식) → SpriteRenderer 추가, Sprite = T_ChargeUp_0
///     ├── TrailNear (자식) → SpriteRenderer 추가, Sprite = T_ChargeUp_0
///     └── Head      (자식) → SpriteRenderer 추가, Sprite = T_ChargeUp_1
///
///   Inspector에서 각 SpriteRenderer 참조를 _srFar, _srNear, _srHead에 연결
/// ──────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(CBullet))]
public class CBulletTrail : MonoBehaviour
{
    [Header("자식 SpriteRenderer 참조 (Inspector에서 연결)")]
    [SerializeField] private SpriteRenderer _srFar;   // TrailFar  자식
    [SerializeField] private SpriteRenderer _srNear;  // TrailNear 자식
    [SerializeField] private SpriteRenderer _srHead;  // Head      자식

    [Header("TrailFar (뒤쪽 넓은 부분)")]
    [SerializeField] private float _farLength = 1.8f;  // X 길이
    [SerializeField] private float _farWidth  = 0.55f; // Y 두께
    [SerializeField] private Color _farColor  = new Color(1f, 0.92f, 0.75f, 0.5f);

    [Header("TrailNear (앞쪽 좁은 부분)")]
    [SerializeField] private float _nearLength = 0.9f;
    [SerializeField] private float _nearWidth  = 0.3f;
    [SerializeField] private Color _nearColor  = new Color(1f, 0.96f, 0.85f, 0.8f);

    [Header("Head (총알 끝 점)")]
    [SerializeField] private float _headSize  = 0.18f;
    [SerializeField] private Color _headColor = new Color(1f, 1f, 1f, 1f);

    [Header("Sorting")]
    [SerializeField] private string _sortingLayerName = "Default";
    [SerializeField] private int    _sortingOrder     = 5;

    #region Public Methods

    /// <summary>CBullet.Init()에서 호출</summary>
    public void StartTrail()
    {
        if (_srFar  != null) _srFar.enabled  = true;
        if (_srNear != null) _srNear.enabled = true;
        if (_srHead != null) _srHead.enabled = true;
    }

    public void StopTrail()
    {
        if (_srFar  != null) _srFar.enabled  = false;
        if (_srNear != null) _srNear.enabled = false;
        if (_srHead != null) _srHead.enabled = false;
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Init 전까지 비주얼 숨김 (첫 프레임 위치 튀는 현상 방지)
        if (_srFar  != null) _srFar.enabled  = false;
        if (_srNear != null) _srNear.enabled = false;
        if (_srHead != null) _srHead.enabled = false;

        ApplySorting();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Init() 호출 시 방향에 맞게 자식 오브젝트 배치
    /// CBullet이 이미 rotation을 설정하므로 여기선 로컬 위치/스케일만 설정
    /// </summary>
    public void SetupVisual()
    {
        // 모든 오브젝트가 자식이므로 회전은 부모(Bullet)에서 상속
        // 로컬 X 음수 방향 = 이동 반대 방향(꼬리)

        if (_srFar != null)
        {
            _srFar.color = _farColor;
            // 뒤쪽으로 멀리: 전체 길이의 중간 지점에 위치시켜 앞쪽 절반이 Head 뒤에 오도록
            _srFar.transform.localPosition = new Vector3(-_farLength * 0.5f, 0f, 0f);
            _srFar.transform.localScale    = new Vector3(_farLength, _farWidth, 1f);
        }

        if (_srNear != null)
        {
            _srNear.color = _nearColor;
            _srNear.transform.localPosition = new Vector3(-_nearLength * 0.5f, 0f, 0f);
            _srNear.transform.localScale    = new Vector3(_nearLength, _nearWidth, 1f);
        }

        if (_srHead != null)
        {
            _srHead.color = _headColor;
            _srHead.transform.localPosition = Vector3.zero;
            _srHead.transform.localScale    = Vector3.one * _headSize;
        }
    }

    private void ApplySorting()
    {
        if (_srFar != null)
        {
            _srFar.sortingLayerName = _sortingLayerName;
            _srFar.sortingOrder     = _sortingOrder;
        }
        if (_srNear != null)
        {
            _srNear.sortingLayerName = _sortingLayerName;
            _srNear.sortingOrder     = _sortingOrder + 1;
        }
        if (_srHead != null)
        {
            _srHead.sortingLayerName = _sortingLayerName;
            _srHead.sortingOrder     = _sortingOrder + 2;
        }
    }

    #endregion
}
