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
    private bool  _hasExploded          = false;
    private float _defaultExplosionRadius;  // Awake 시 인스펙터 값을 캐싱 — 풀 반환 시 복원 기준
    #endregion

    protected override void Awake()
    {
        base.Awake();
        _defaultExplosionRadius = _explosionRadius; // 인스펙터 설정값 보존
    }

    protected override bool UseDeathAnimation() => false; // 자폭 이펙트가 연출을 대체

    public override void Die()
    {
        Explode();
        base.Die(); // OnDied 이벤트 발행 → CSpawnManager가 풀 반환
    }

    public override void ResetForPool()
    {
        base.ResetForPool();
        _hasExploded     = false;
        _explosionRadius = _defaultExplosionRadius; // 인스펙터 원본값으로 복원
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
