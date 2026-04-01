using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRangedController : CEnemyBase
{
    #region 인스펙터
    [Header("참조")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    #endregion

    protected override void Start()
    {
        base.Start();

        if (IsPersonalScene)
        {
            CPlayerController player = FindFirstObjectByType<CPlayerController>();

            if (player != null)
            {
                SetTarget(player.transform);
                Debug.Log($"타겟 : {player.name}");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} : 플레이어를 찾을 수 없음");
            }

            InitEnemy(1);
        }
    }

    protected override void HandleMovement()
    {
        if (HasStatus(EStatusEffect.Knockback)) return;

        if (CurrentTarget == null)
        {
            Rb.velocity = Vector2.zero;
            return;
        }

        float distance      = Vector2.Distance(transform.position, CurrentTarget.position);
        Vector2 dirToPlayer = (CurrentTarget.position - transform.position).normalized;

        FlipCharacter(dirToPlayer.x);

        if (distance > AttackRange)
            Rb.velocity = dirToPlayer * MoveSpeed;  // 사거리 밖 → 접근
        else
            Rb.velocity = Vector2.zero;              // 사거리 안 → 정지 후 공격
    }

    protected override void ExecuteAttack()
    {
        if (_projectilePrefab == null || CurrentTarget == null) return;

        Vector2 dir = (CurrentTarget.position - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
        GameObject obj = Instantiate(_projectilePrefab, spawnPos, rotation);

        CProjectileTest projectile = obj.GetComponent<CProjectileTest>();

        if (projectile != null)
        {
            projectile.Init(AttackDamage, dir);
        }
    }
}
