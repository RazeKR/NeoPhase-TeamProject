using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CProjectileExplosion : MonoBehaviour
{
    public float explosionRadius = 1f;
    public LayerMask enemyLayer;

    private float _damage;
    private GameObject _owner;

    // Init on TimeToLive
    public void Init(float damage, GameObject owner, float scale)
    {
        _damage = damage;
        _owner = owner;
        transform.localScale = Vector3.one * scale;

        Explode();
    }

    private void Explode()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable target = enemy.GetComponent<IDamageable>();
            if (target != null)
            {
                Vector2 hitDir = (enemy.transform.position - transform.position).normalized;

                target.TakeDamage(_damage, hitDir);
            }
        }

        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius * transform.localScale.x);
    }
}
