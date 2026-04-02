using UnityEngine;

public class CSkillKnockback : MonoBehaviour
{
    [Header("넉백 설정")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        CEntityBase entity = other.GetComponentInParent<CEntityBase>();
        if (entity == null) return;

        Vector2 dir = (other.transform.position - transform.position).normalized;
        entity.ApplyKnockback(dir * knockbackForce, knockbackDuration);
    }
}
