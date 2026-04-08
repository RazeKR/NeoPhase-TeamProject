using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CThunder : MonoBehaviour
{
    #region 인스펙터
    [Header("피격 범위")]
    [SerializeField] private float _strikeRadius = 1.5f;
    #endregion

    #region 내부 변수
    private float _damage;
    private LayerMask _enemyLayer;
    #endregion

    public void Init(float damage, LayerMask enemyLayer)
    {
        _damage = damage;
        _enemyLayer = enemyLayer;

        Strike();

        Invoke(nameof(ReturnToPool), 0.7f);
    }

    private void Strike()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, _strikeRadius, _enemyLayer);

        foreach (Collider2D col in targets)
        {
            if (col.TryGetComponent(out IDamageable target))
            {
                Vector2 hitDir = (col.transform.position - transform.position).normalized;
                Transform targetTr = col.transform;
                CFogFlashSource.SpawnImpact(targetTr.position, outerRadius: 2f, peakIntensity: 0.5f);
                target.TakeDamage(_damage, hitDir);
            }
        }
    }

    private void ReturnToPool()
    {
        CThunderPoolManager.Instance.Return(this);
    }
}
