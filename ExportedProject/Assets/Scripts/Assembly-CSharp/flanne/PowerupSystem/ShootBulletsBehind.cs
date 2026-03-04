using UnityEngine;

namespace flanne.PowerupSystem
{
	public class ShootBulletsBehind : MonoBehaviour
	{
		[SerializeField]
		private int numOfBullets;

		[SerializeField]
		private float spread;

		[SerializeField]
		private float damageMultiplier;

		private ProjectileFactory PF;

		private Gun myGun;

		private ShootingCursor SC;

		private void Start()
		{
			PF = ProjectileFactory.SharedInstance;
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			myGun = componentInParent.gun;
			myGun.OnShoot.AddListener(OnShoot);
			SC = ShootingCursor.Instance;
		}

		private void OnDestroy()
		{
			myGun.OnShoot.RemoveListener(OnShoot);
		}

		private void OnShoot()
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 v = (Vector2)base.transform.position - vector;
			float num = -1f * spread / 2f;
			for (int i = 0; i < numOfBullets; i++)
			{
				float degrees = num + (float)i / (float)numOfBullets * spread;
				Vector2 direction = v.Rotate(degrees);
				PF.SpawnProjectile(myGun.GetProjectileRecipe(), direction, base.transform.position, damageMultiplier);
			}
		}
	}
}
