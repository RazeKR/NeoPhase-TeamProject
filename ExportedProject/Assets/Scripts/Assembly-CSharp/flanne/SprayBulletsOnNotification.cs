using UnityEngine;

namespace flanne
{
	public class SprayBulletsOnNotification : MonoBehaviour
	{
		[SerializeField]
		private string notification;

		[SerializeField]
		private int numOfBullets;

		[SerializeField]
		private float damageMultiplier;

		private ProjectileFactory PF;

		private PlayerController player;

		private void OnNotification(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			ShootBullets(gameObject.transform.position);
		}

		private void Start()
		{
			PF = ProjectileFactory.SharedInstance;
			player = GetComponentInParent<PlayerController>();
			this.AddObserver(OnNotification, notification);
		}

		private void OnDisable()
		{
			this.RemoveObserver(OnNotification, notification);
		}

		private void ShootBullets(Vector3 spawnPos)
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
				projectileRecipe.knockback *= 0.1f;
				projectileRecipe.piercing++;
				PF.SpawnProjectile(projectileRecipe, direction, spawnPos, damageMultiplier);
			}
		}
	}
}
