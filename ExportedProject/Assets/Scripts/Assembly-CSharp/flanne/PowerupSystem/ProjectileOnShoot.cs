using UnityEngine;

namespace flanne.PowerupSystem
{
	public class ProjectileOnShoot : AttackOnShoot
	{
		[SerializeField]
		private GameObject projectilePrefab;

		[SerializeField]
		private float speed = 20f;

		[SerializeField]
		private float inaccuracy = 45f;

		public int numProjectiles = 1;

		[SerializeField]
		private bool setToGunDamage;

		[SerializeField]
		private float percentOfGunDamage;

		[SerializeField]
		private bool projHasPeriodicDamage;

		public float periodicDamageFrequency;

		private ObjectPooler OP;

		private ShootingCursor SC;

		private Gun gun;

		protected override void Init()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(projectilePrefab.name, projectilePrefab, 50);
			SC = ShootingCursor.Instance;
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			gun = componentInParent.gun;
		}

		public override void Attack()
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 vector2 = base.transform.position;
			Vector2 v = vector - vector2;
			float num = -1f * inaccuracy / 2f;
			float max = -1f * num;
			if (numProjectiles > 1)
			{
				for (int i = 0; i < numProjectiles; i++)
				{
					float degrees = num + (float)i / (float)(numProjectiles - 1) * inaccuracy;
					Vector2 direction = v.Rotate(degrees);
					SpawnProjectile(direction);
				}
			}
			else
			{
				Vector2 direction2 = v.Rotate(Random.Range(num, max));
				SpawnProjectile(direction2);
			}
		}

		private void SpawnProjectile(Vector2 direction)
		{
			GameObject pooledObject = OP.GetPooledObject(projectilePrefab.name);
			pooledObject.SetActive(value: true);
			Projectile component = pooledObject.GetComponent<Projectile>();
			component.vector = speed * direction.normalized;
			component.angle = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
			if (setToGunDamage)
			{
				component.damage = percentOfGunDamage * gun.damage;
			}
			Vector2 vector = direction.normalized * 0.7f;
			pooledObject.transform.position = base.transform.position + new Vector3(vector.x, vector.y, 0f);
			if (projHasPeriodicDamage)
			{
				PeriodicGameObjectActivator component2 = pooledObject.GetComponent<PeriodicGameObjectActivator>();
				if (component2 != null)
				{
					component2.timeBetweenActivations = periodicDamageFrequency;
				}
			}
		}
	}
}
