using UnityEngine;

/// <summary>
/// 플레이어 무기 투사체 — 이동과 충돌만 담당
/// Head(앞 원형) + Tail(뒤 타원형)을 조합해 빔 형태를 표현
/// Init() 호출 시 방향/데미지/속도/수명이 주입된다
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CBullet : MonoBehaviour
{
    #region 인스펙터

    [Header("스프라이트 참조")]
    [Tooltip("앞쪽 둥근 머리 스프라이트 렌더러 (T_ChargeUp_0 — 큰 원)")]
    [SerializeField] private SpriteRenderer _headRenderer;   // 자식 오브젝트

    [Tooltip("뒤쪽 꼬리 스프라이트 렌더러 (T_ChargeUp_1 — 작은 원)")]
    [SerializeField] private SpriteRenderer _tailRenderer;   // 자식 오브젝트

    [Header("머리(Head) 설정")]
    [Tooltip("머리 크기 — 클수록 앞이 두툼하게 보임 (0.3~0.6 권장)")]
    [SerializeField] [Range(0.1f, 1f)] private float _headScale = 0.45f;

    [Header("꼬리(Tail) 설정")]
    [Tooltip("꼬리 X축 길이 — 클수록 뒤로 길게 늘어남 (1.5~4 권장)")]
    [SerializeField] [Range(0.5f, 6f)] private float _tailLengthX = 3f;

    [Tooltip("꼬리 Y축 두께 — 작을수록 뾰족해짐 (0.05~0.2 권장)")]
    [SerializeField] [Range(0.02f, 0.4f)] private float _tailThicknessY = 0.12f;

    [Tooltip("꼬리가 머리 뒤에서 시작하는 오프셋 (음수 = 뒤쪽)")]
    [SerializeField] private float _tailOffsetX = -0.3f;

    [Header("공통")]
    [Tooltip("타일맵 위에 렌더링되도록 Sorting Order 값 지정")]
    [SerializeField] private int _sortingOrder = 10;

    #endregion

    #region 내부 변수

    private float   _damage;
    private float   _lifeTime;
    private float   _spawnTime;
    private bool    _initialized;
    private Vector2 _direction; // 발사 방향 — 충돌 시 hitDir로 전달하기 위해 Init에서 저장

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (_headRenderer != null) _headRenderer.sortingOrder = _sortingOrder;
        if (_tailRenderer != null) _tailRenderer.sortingOrder = _sortingOrder - 1; // 꼬리는 머리 아래
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 투사체 초기화 — 스폰 직후 반드시 호출해야 한다
    /// </summary>
    public void Init(Vector2 direction, float damage, float speed, float lifeTime)
    {
        _damage      = damage;
        _lifeTime    = lifeTime;
        _spawnTime   = Time.time;
        _initialized = true;

        // ── 이동 방향 설정 ──────────────────────────────────────
        Vector2 dir = direction.normalized;
        _direction   = dir; // 충돌 시 피격 방향으로 전달하기 위해 저장
        GetComponent<Rigidbody2D>().velocity = dir * speed;

        // 전체 오브젝트를 진행 방향으로 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation   = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.localScale = Vector3.one; // 루트는 스케일 건드리지 않음

        // ── 머리(Head): 앞쪽 둥근 원 ────────────────────────────
        if (_headRenderer != null)
        {
            _headRenderer.transform.localPosition = Vector3.zero;
            _headRenderer.transform.localScale    = new Vector3(_headScale, _headScale, 1f);
            _headRenderer.transform.localRotation = Quaternion.identity;
        }

        // ── 꼬리(Tail): 뒤쪽으로 늘어나는 타원 ──────────────────
        if (_tailRenderer != null)
        {
            // _tailLengthX 의 절반만큼 뒤로 이동해 머리에 붙인다
            float tailCenterX = _tailOffsetX - (_tailLengthX * 0.5f);
            _tailRenderer.transform.localPosition = new Vector3(tailCenterX, 0f, 0f);
            _tailRenderer.transform.localScale    = new Vector3(_tailLengthX, _tailThicknessY, 1f);
            _tailRenderer.transform.localRotation = Quaternion.identity;
        }
    }

    #endregion

    #region Unity Methods (Update / Collision)

    private void Update()
    {
        if (!_initialized) return;

        if (Time.time - _spawnTime >= _lifeTime)
            DestroyBullet();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_initialized) return;
        if (other.GetComponent<CPlayerController>() != null) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            // _direction : Init에서 저장한 발사 방향을 hitDir로 전달하여 HitFlash·데미지텍스트 연출 활성화
            damageable.TakeDamage(_damage, _direction);
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

    #endregion
}
