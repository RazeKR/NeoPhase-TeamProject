using UnityEngine;

public class CBoomerController : CEnemyBase
{
    #region 인스펙터
    [Header("자폭 옵션")]
    [SerializeField] private GameObject _explosionEffectPrefab;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private LayerMask _explosionLayer;
    #endregion

    #region 내부 변수
    private bool _hasExploded = false;
    #endregion

    protected override void Start()
    {
        base.Start();

        _explosionRadius = _attackRange;
    }

    public override void Die()
    {
        Explode();

        base.Die();
    }
    
    protected override void ExecuteAttack()
    {
        Explode();
    }

    /// <summary>
    /// 자폭 시 실행
    /// </summary>
    private void Explode()
    {
        if (_hasExploded) return;

        _hasExploded = true;

        if (_explosionEffectPrefab != null)
        {
            Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _explosionLayer);

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == this.gameObject) continue;

            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_attackDamage);
            }
        }

        Destroy(gameObject);
    }
}
