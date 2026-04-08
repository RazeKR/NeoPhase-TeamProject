using UnityEngine;

public class CProjectileTest : MonoBehaviour
{
    #region 인스펙터
    [Header("투사체 속도")]
    [SerializeField] private float _speed    = 8f;
    [Header("수명 (초)")]
    [SerializeField] private float _lifeTime = 5f;
    #endregion

    #region 내부 변수
    private float _damage;
    private float _spawnTime;
    #endregion

    #region 이벤트
    public static event System.Action<CProjectileTest> OnProjectileReturned;
    #endregion

    private void OnDisable()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void Init(float damage, Vector2 direction)
    {
        _damage    = damage;
        _spawnTime = Time.time;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = direction.normalized * _speed;
    }

    private void Update()
    {
        if (Time.time - _spawnTime >= _lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 적(발사자 포함)에는 피해 없음
        if (collision.GetComponentInParent<CEnemyBase>() != null) return;

        IDamageable target = collision.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(_damage);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        OnProjectileReturned?.Invoke(this);
    }
}
