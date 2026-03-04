using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class Shooter : MonoBehaviour
	{
		public static string BulletShotEvent = "Shooter.BulletShotEvent";

		public static string TweakProjectileRecipe = "Shooter.TweakProjectileRecipe";

		[SerializeField]
		private GameObject muzzleFlashObject;

		public UnityEvent onShoot;

		protected ProjectileFactory PF;

		protected ObjectPooler OP;

		public virtual bool fireOnStop => false;

		private void Start()
		{
			PF = ProjectileFactory.SharedInstance;
			OP = ObjectPooler.SharedInstance;
			Init();
		}

		public virtual void Init()
		{
		}

		public virtual void OnStopShoot(ProjectileRecipe recipe, Vector2 pointDirection, int numProjectiles, float spread, float inaccuracy)
		{
		}

		public virtual void Shoot(ProjectileRecipe recipe, Vector2 pointDirection, int numProjectiles, float spread, float inaccuracy)
		{
			this.PostNotification(TweakProjectileRecipe, recipe);
			pointDirection = RandomizeDirection(pointDirection, inaccuracy);
			if (numProjectiles > 1)
			{
				spread = Mathf.Max(spread, 5f);
				float num = -1f * (spread / 2f);
				for (int i = 0; i < numProjectiles; i++)
				{
					float degrees = num + (float)i / (float)(numProjectiles - 1) * spread;
					Vector2 direction = pointDirection.Rotate(degrees);
					Projectile e = PF.SpawnProjectile(recipe, direction, base.transform.position);
					this.PostNotification(BulletShotEvent, e);
				}
			}
			else
			{
				Projectile e2 = PF.SpawnProjectile(recipe, pointDirection, base.transform.position);
				this.PostNotification(BulletShotEvent, e2);
			}
			if (muzzleFlashObject != null)
			{
				muzzleFlashObject?.SetActive(value: true);
			}
			onShoot?.Invoke();
		}

		private Vector3 RandomizeDirection(Vector2 direction, float degrees)
		{
			float num = -1f * degrees / 2f;
			float min = -1f * num;
			Vector2 v = new Vector2(direction.x, direction.y);
			v = v.Rotate(Random.Range(min, num));
			return new Vector3(v.x, v.y, 0f);
		}
	}
}
