using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class VolleyTowardsCursorAction : Action
	{
		[SerializeField]
		private float bulletSpawnOffset;

		[SerializeField]
		private float spread;

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

		private ProjectileFactory PF;

		private Gun myGun;

		private ShootingCursor SC;

		private int _activationCount;

		public override void Init()
		{
			PF = ProjectileFactory.SharedInstance;
			myGun = PlayerController.Instance.gun;
			SC = ShootingCursor.Instance;
		}

		public override void Activate(GameObject target)
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 vector2 = myGun.transform.position;
			Vector2 v = (vector - vector2).Rotate(-1f * (spread / 2f));
			for (int i = 0; i < numOfBullets; i++)
			{
				float degrees = (float)i / (float)numOfBullets * spread;
				Vector2 vector3 = v.Rotate(degrees);
				ProjectileRecipe projectileRecipe = myGun.GetProjectileRecipe();
				projectileRecipe.size = sizeMultiplier;
				projectileRecipe.knockback *= knockbackMultiplier;
				projectileRecipe.projectileSpeed *= projSpeedMultiplier;
				Vector3 position = myGun.transform.position;
				PF.SpawnProjectile(projectileRecipe, vector3, position + (Vector3)(vector3 * bulletSpawnOffset), damageMultiplier, unretrievableByMagicBow);
			}
		}
	}
}
