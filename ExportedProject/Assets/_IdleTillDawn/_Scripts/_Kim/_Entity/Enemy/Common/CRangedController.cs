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

    protected override void ExecuteAttack()
    {
        if (_projectilePrefab == null || CurrentTarget == null) return;

        Vector2 dir = (CurrentTarget.position - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject obj = Instantiate(_projectilePrefab, _firePoint.position, rotation);

        CProjectileTest projectile = obj.GetComponent<CProjectileTest>();

        if (projectile != null)
        {
            projectile.Init(AttackDamage, dir);
        }
    }
}
