using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMeleeController : CEnemyBase
{
    //protected override void Start()
    //{
    //    base.Start();

    //    CPlayerController player = FindFirstObjectByType<CPlayerController>();

    //    if (player != null)
    //    {
    //        SetTarget(player.transform);
    //        Debug.Log($"타겟 : {player.name}");
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"{gameObject.name} : 플레이어를 찾을 수 없음");
    //    }

    //    InitEnemy(1);
    //}

    protected override void ExecuteAttack()
    {
        if (CurrentHealth <= 0) return;
        
        if (TargetDamageable != null)
        {
            TargetDamageable.TakeDamage(AttackDamage);
        }
    }
}
