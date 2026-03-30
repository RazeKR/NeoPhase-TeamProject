using UnityEngine;

/// <summary>
/// 플레이어 무기 투사체 — 이동과 충돌만 담당
/// Init() 호출 시 방향/데미지/속도/수명이 주입된다
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CBullet : MonoBehaviour
{
    #region 내부 변수

    private float          _damage;
    private float          _lifeTime;
    private float          _spawnTime;
    private bool           _initialized;
    private CBulletTrail   _trail;

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
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.localScale = Vector3.one;

        if (_trail != null)
        {
            _trail.SetupVisual();
            _trail.StartTrail();
        }
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _trail = GetComponent<CBulletTrail>();
    }

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
        if (_trail != null)
            _trail.StopTrail();

        Destroy(gameObject);
    }

    #endregion
}
