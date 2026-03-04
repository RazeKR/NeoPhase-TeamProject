using UnityEngine;

namespace flanne
{
	public class ShootingSummon : AttackingSummon
	{
		[SerializeField]
		private GameObject projectilePrefab;

		[SerializeField]
		private Shooter shooter;

		public bool targetMouse;

		public bool inheritPlayerDamage;

		public int baseDamage;

		public int numProjectiles;

		public float projectileSpeed;

		public float knockback;

		public int bounce;

		public int pierce;

		private ObjectPooler OP;

		private ShootingCursor SC;

		private void OnHit(object sender, object args)
		{
			GameObject e = args as GameObject;
			this.PostNotification(Summon.SummonOnHitNotification, e);
		}

		protected override void Init()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(projectilePrefab.name, projectilePrefab, 50);
			SC = ShootingCursor.Instance;
			this.AddObserver(OnHit, Projectile.ImpactEvent, base.gameObject);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnHit, Projectile.ImpactEvent, base.gameObject);
		}

		protected override bool Attack()
		{
			Vector2 vector = base.transform.position;
			_ = Vector2.zero;
			if (targetMouse)
			{
				Vector2 direction = (Vector2)Camera.main.ScreenToWorldPoint(SC.cursorPosition) - vector;
				Shoot(direction);
				return true;
			}
			if (EnemyFinder.GetRandomEnemy(vector, new Vector2(9f, 6f)) != null)
			{
				Vector2 direction2 = (Vector2)EnemyFinder.GetRandomEnemy(vector, new Vector2(9f, 6f)).transform.position - vector;
				Shoot(direction2);
				return true;
			}
			return false;
		}

		private void Shoot(Vector2 direction)
		{
			if (direction.x < 0f)
			{
				base.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
			else if (direction.x > 0f)
			{
				base.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			shooter.Shoot(GetProjectileRecipe(), direction, numProjectiles, (numProjectiles - 1) * 15, 0f);
		}

		private ProjectileRecipe GetProjectileRecipe()
		{
			ProjectileRecipe obj = new ProjectileRecipe
			{
				objectPoolTag = projectilePrefab.name
			};
			float f = ((!inheritPlayerDamage) ? base.summonDamageMod.Modify(baseDamage) : base.summonDamageMod.Modify(player.gun.damage));
			obj.damage = ApplyDamageMods(Mathf.FloorToInt(f));
			obj.projectileSpeed = projectileSpeed;
			obj.size = 1f;
			obj.knockback = knockback;
			obj.bounce = bounce;
			obj.piercing = pierce;
			obj.owner = base.gameObject;
			return obj;
		}
	}
}
