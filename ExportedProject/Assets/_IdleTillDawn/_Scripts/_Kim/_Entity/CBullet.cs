using UnityEngine;

/// <summary>
/// 플레이어 무기 투사체 — 이동과 충돌만 담당
/// Init() 호출 시 방향/데미지/속도/수명이 주입된다
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CBullet : MonoBehaviour
{
    #region 인스펙터

    [Tooltip("진행 방향(X축) 스트레치 배율 (4~6 권장 — 레이저처럼 보이지 않는 범위)")]
    [SerializeField] [Range(3f, 6f)] private float _stretchX = 5f;

    [Tooltip("Y축 두께 (0.15~0.3 권장 — 얇게 표현)")]
    [SerializeField] [Range(0.1f, 0.5f)] private float _stretchY = 0.22f;

    [Tooltip("타일맵 위에 렌더링되도록 Sorting Order 값 지정")]
    [SerializeField] private int _sortingOrder = 10;

    #endregion

    #region 내부 변수

    private float            _damage;
    private float            _lifeTime;
    private float            _spawnTime;
    private bool             _initialized;
    private SpriteRenderer   _sr;

    #endregion

    #region Unity Methods (Awake)

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null)
            _sr.sortingOrder = _sortingOrder;
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

        Vector2 dir = direction.normalized;
        GetComponent<Rigidbody2D>().velocity = dir * speed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation   = Quaternion.AngleAxis(angle, Vector3.forward);
        // X: 진행 방향 스트레치 / Y: 얇은 두께 → 끝이 원, 뒤쪽이 가늘어지는 느낌
        transform.localScale = new Vector3(_stretchX, _stretchY, 1f);
    }

    #endregion

    #region Unity Methods

    private void Update()
    {
        if (!_initialized) return;

        if (Time.time - _spawnTime >= _lifeTime)
        {
            DestroyBullet();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_initialized) return;
        if (other.GetComponent<CPlayerController>() != null) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

    #endregion
}
