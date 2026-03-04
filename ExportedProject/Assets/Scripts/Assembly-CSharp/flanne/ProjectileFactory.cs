using UnityEngine;

namespace flanne
{
	public class ProjectileFactory : MonoBehaviour
	{
		public static ProjectileFactory SharedInstance;

		private ObjectPooler OP;

		private void Awake()
		{
			SharedInstance = this;
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
		}

		public Projectile SpawnProjectile(ProjectileRecipe recipe, Vector2 direction, Vector3 position, float damageMultiplier = 1f, bool isSecondary = false)
		{
			GameObject pooledObject = OP.GetPooledObject(recipe.objectPoolTag);
			pooledObject.SetActive(value: true);
			pooledObject.transform.position = position;
			Projectile component = pooledObject.GetComponent<Projectile>();
			component.isSecondary = isSecondary;
			component.vector = recipe.projectileSpeed * direction.normalized;
			component.angle = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
			component.size = recipe.size;
			component.damage = Mathf.Max(1f, recipe.damage * damageMultiplier);
			component.knockback = recipe.knockback;
			component.bounce = recipe.bounce;
			component.piercing = recipe.piercing;
			component.owner = recipe.owner;
			return component;
		}
	}
}
