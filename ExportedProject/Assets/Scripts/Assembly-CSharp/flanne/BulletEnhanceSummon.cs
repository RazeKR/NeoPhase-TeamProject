using System;
using UnityEngine;

namespace flanne
{
	public class BulletEnhanceSummon : Summon
	{
		[NonSerialized]
		public float overallMultiplier = 1f;

		public float damageMultiplier;

		public float sizeMultiplier = 1f;

		public int additionalBounces;

		public bool addBurn;

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag == "Bullet")
			{
				Projectile component = other.gameObject.GetComponent<Projectile>();
				if (!(component == null))
				{
					ModifyProjectile(component);
				}
			}
		}

		private void ModifyProjectile(Projectile projectile)
		{
			projectile.damage += base.summonDamageMod.Modify(player.gun.damage * damageMultiplier) * overallMultiplier;
			projectile.size = projectile.transform.localScale.x * (sizeMultiplier * overallMultiplier);
			projectile.bounce += Mathf.FloorToInt((float)additionalBounces * overallMultiplier);
			if (addBurn)
			{
				BurnOnCollision burnOnCollision = projectile.gameObject.AddComponent<BurnOnCollision>();
				burnOnCollision.burnDamage = Mathf.FloorToInt(3f * overallMultiplier);
				burnOnCollision.hitTag = "Enemy";
			}
		}
	}
}
