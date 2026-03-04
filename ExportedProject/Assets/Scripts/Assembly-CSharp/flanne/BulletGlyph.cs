using System.Collections;
using UnityEngine;

namespace flanne
{
	public class BulletGlyph : Spawn
	{
		[SerializeField]
		private float timeToActivate;

		[SerializeField]
		private int numOfBullets;

		[SerializeField]
		private float baseDamageMultiplier;

		[SerializeField]
		private GameObject knockbackObject;

		[SerializeField]
		private SoundEffectSO soundFX;

		private ProjectileFactory PF;

		private float damageMultiplier => player.stats[StatType.SummonDamage].Modify(baseDamageMultiplier);

		private void Start()
		{
			PF = ProjectileFactory.SharedInstance;
		}

		private void OnEnable()
		{
			StartCoroutine(LifetimeCR());
		}

		private void ShootBullets()
		{
			Vector2 vector = Vector2.zero;
			while (vector == Vector2.zero)
			{
				vector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			}
			for (int i = 0; i < numOfBullets; i++)
			{
				float degrees = (float)i / (float)numOfBullets * 360f;
				Vector2 direction = vector.Rotate(degrees);
				ProjectileRecipe projectileRecipe = player.gun.GetProjectileRecipe();
				projectileRecipe.size *= 0.5f;
				projectileRecipe.knockback *= 0.1f;
				PF.SpawnProjectile(projectileRecipe, direction, base.transform.position, damageMultiplier, isSecondary: true);
			}
		}

		private IEnumerator LifetimeCR()
		{
			yield return new WaitForSeconds(timeToActivate - 0.1f);
			knockbackObject.SetActive(value: true);
			yield return new WaitForSeconds(0.1f);
			ShootBullets();
			knockbackObject.SetActive(value: false);
			soundFX?.Play();
			base.gameObject.SetActive(value: false);
		}
	}
}
