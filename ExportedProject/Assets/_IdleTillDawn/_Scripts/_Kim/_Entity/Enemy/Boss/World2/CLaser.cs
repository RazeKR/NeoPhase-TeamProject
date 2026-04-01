using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLaser : MonoBehaviour
{
	#region 인스펙터
	[Header("보스 참조")]
	[SerializeField] private CBossBase _boss;
	#endregion

    private void OnTriggerStay2D(Collider2D collision)
    {
		if (!this.enabled) return;

		if (collision.GetComponentInParent<CEnemyBase>() != null) return;

		IDamageable target = collision.GetComponent<IDamageable>();
			
		if (target != null)
		{
			target.TakeDamage(_boss.AttackDamage);
		}
    }
}
