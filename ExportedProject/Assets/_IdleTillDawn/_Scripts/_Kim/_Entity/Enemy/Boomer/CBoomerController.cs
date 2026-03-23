using UnityEngine;

public class CBoomerController : CEnemyBase
{
    #region 인스펙터
    [Header("자폭 옵션")]
    [SerializeField] private GameObject _explosionEffectPrefab;
    [SerializeField] private float      _explosionRadius;
    [SerializeField] private LayerMask  _explosionLayer;
    #endregion

    #region 내부 변수
    private bool _hasExploded = false;
    #endregion

    protected override void Start()
    {
        base.Start();
        _explosionRadius = AttackRange;
    }

    public override void Die()
    {
        Explode();
        base.Die(); // OnDied 이벤트 발행 → CSpawnManager가 풀 반환
    }

    public override void ResetForPool()
    {
        base.ResetForPool();
        _hasExploded     = false;
        _explosionRadius = 0f; // InitEnemy 후 Start에서 _attackRange로 재설정됨
    }

    protected override void ExecuteAttack()
    {
        Die();
    }

    /// <summary>
    /// 자폭 — 범위 내 IDamageable에 데미지 후 풀 반환 위임
    /// </summary>
    private void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        if (_explosionEffectPrefab != null)
            Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _explosionLayer);

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == this.gameObject) continue;

            IDamageable damageable = col.GetComponent<IDamageable>();
            damageable?.TakeDamage(AttackDamage);
        }

        // Destroy 제거 — Die() → base.Die() → OnDied → CSpawnManager.ReturnToPool 로 처리
    }
}
