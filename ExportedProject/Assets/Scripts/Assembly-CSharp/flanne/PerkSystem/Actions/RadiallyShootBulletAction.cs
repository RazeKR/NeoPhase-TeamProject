using System.Collections;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class RadiallyShootBulletAction : Action
	{
		[SerializeField]
		private float bulletSpawnOffset;

		[SerializeField]
		private float delayBetweenShots;

		[SerializeField]
		private int numOfBullets = 1;

		[SerializeField]
		private float damageMultiplier = 1f;

		[SerializeField]
		private float sizeMultiplier = 1f;

		[SerializeField]
		private float knockbackMultiplier = 1f;

		[SerializeField]
		private float projSpeedMultiplier = 1f;

		[SerializeField]
		private bool unretrievableByMagicBow;

		[SerializeField]
		private bool limitActivationsPerFrame;

		[SerializeField]
		private int activationLimit;

		private ProjectileFactory PF;

		private Gun myGun;

		private int _activationCount;

		public override void Init()
		{
			PF = ProjectileFactory.SharedInstance;
			myGun = PlayerController.Instance.gun;
		}

		public override void Activate(GameObject target)
		{
			if (!limitActivationsPerFrame || _activationCount < activationLimit)
			{
				PlayerController.Instance.StartCoroutine(RadiallyShootCR(target.transform));
			}
		}

		private IEnumerator RadiallyShootCR(Transform center)
		{
			_activationCount++;
			Vector2 startDirection = Vector2.zero;
			while (startDirection == Vector2.zero)
			{
				startDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
			}
			for (int i = 0; i < numOfBullets; i++)
			{
				float degrees = (float)i / (float)numOfBullets * 360f;
				Vector2 vector = startDirection.Rotate(degrees);
				ProjectileRecipe projectileRecipe = myGun.GetProjectileRecipe();
				projectileRecipe.size = sizeMultiplier;
				projectileRecipe.knockback *= knockbackMultiplier;
				projectileRecipe.projectileSpeed *= projSpeedMultiplier;
				Vector3 position = center.position;
				PF.SpawnProjectile(projectileRecipe, vector, position + (Vector3)(vector * bulletSpawnOffset), damageMultiplier, unretrievableByMagicBow);
				yield return new WaitForSeconds(delayBetweenShots);
			}
			_activationCount--;
		}
	}
}
