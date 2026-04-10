using System.Collections.Generic;
using UnityEngine;

public class CDeerRamAttack : MonoBehaviour
{
    #region 프로퍼티
    public float Damage { get; set; }
    public LayerMask EnemyLayer { get; set; }
    public float KnockbackForce { get; set; }
    public float KnockbackDuration { get; set; }
    #endregion

    // 적별 마지막 넉백 적용 시각 (Stay 중 쿨다운 관리)
    private readonly Dictionary<int, float> _hitCooldowns = new Dictionary<int, float>();
    private const float HitCooldown = 0.15f;

    private void Awake()
    {
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.05f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryTouchAttack(collision.gameObject, firstHit: true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryTouchAttack(collision.gameObject, firstHit: false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _hitCooldowns.Remove(collision.gameObject.GetInstanceID());
    }

    private void TryTouchAttack(GameObject targetObj, bool firstHit)
    {
        if (((1 << targetObj.layer) & EnemyLayer.value) == 0) return;

        CEnemyBase target = targetObj.GetComponentInParent<CEnemyBase>();
        if (target == null) return;

        int id = target.gameObject.GetInstanceID();

        // 쿨다운 체크 (첫 타격은 무조건 적용)
        if (!firstHit)
        {
            if (_hitCooldowns.TryGetValue(id, out float lastHit) && Time.time - lastHit < HitCooldown)
                return;
        }

        _hitCooldowns[id] = Time.time;

        // 플레이어 중심 → 적 방향으로 원형 넉백
        // 완전히 겹쳐있으면 적의 콜라이더 경계 방향으로 보정
        Vector2 playerCenter = (Vector2)transform.position;
        Vector2 enemyCenter  = (Vector2)target.transform.position;
        Vector2 hitDir       = enemyCenter - playerCenter;

        if (hitDir.sqrMagnitude < 0.0001f)
        {
            // 겹침 : 적 콜라이더의 closestPoint로 방향 재계산
            Collider2D enemyCol = target.GetComponentInChildren<Collider2D>();
            if (enemyCol != null)
                hitDir = (Vector2)enemyCol.ClosestPoint(playerCenter + Vector2.up * 0.01f) - playerCenter;

            // 그래도 0이면 랜덤 방향으로 분리
            if (hitDir.sqrMagnitude < 0.0001f)
                hitDir = Random.insideUnitCircle.normalized;
        }

        hitDir = hitDir.normalized;

        bool isBoss = target is CBossBase;

        // 첫 타격에만 데미지 및 Fog 적용
        if (firstHit)
        {
            target.TakeDamage(Damage, hitDir);
            CFogFlashSource.SpawnImpact(target.transform.position,
                outerRadius: isBoss ? 4f : 2f,
                peakIntensity: isBoss ? 1f : 0.5f);
            CDebug.Log($"[사슴 박치기] '{target.EntityName}'에게 {Damage} 데미지!");
        }

        // 넉백은 Stay 동안 계속 재적용하여 플레이어와 겹치지 않도록 분리
        target.Rb.velocity = hitDir * KnockbackForce;
    }
}
