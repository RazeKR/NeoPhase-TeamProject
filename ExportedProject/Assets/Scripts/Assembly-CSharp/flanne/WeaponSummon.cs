using UnityEngine;

namespace flanne
{
	public class WeaponSummon : Summon
	{
		public int baseDamage = 50;

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains("Enemy"))
			{
				Health component = other.gameObject.GetComponent<Health>();
				if (!(component == null))
				{
					int damage = Mathf.FloorToInt(base.summonDamageMod.Modify(baseDamage));
					damage = ApplyDamageMods(damage);
					component.TakeDamage(DamageType.Summon, damage);
					this.PostNotification(Summon.SummonOnHitNotification, other.gameObject);
				}
			}
		}
	}
}
