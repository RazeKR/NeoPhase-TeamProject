using System;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ThrowProjectileTowardsCursorAction : Action
	{
		[SerializeField]
		private GameObject projectilePrefab;

		[SerializeField]
		private int numProjectiles = 1;

		[SerializeField]
		private float speed = 20f;

		[SerializeField]
		private float inaccuracy = 45f;

		[SerializeField]
		private float spawnOffset = 0.7f;

		[SerializeField]
		private bool lockRotation;

		[NonSerialized]
		private ObjectPooler OP;

		[NonSerialized]
		private ShootingCursor SC;

		[NonSerialized]
		private PlayerController player;

		public override void Init()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(projectilePrefab.name, projectilePrefab, 30);
			SC = ShootingCursor.Instance;
			player = PlayerController.Instance;
		}

		public override void Activate(GameObject target)
		{
			Vector2 directionToCursor = GetDirectionToCursor();
			float num = -1f * inaccuracy / 2f;
			float max = -1f * num;
			if (numProjectiles > 1)
			{
				for (int i = 0; i < numProjectiles; i++)
				{
					float degrees = num + (float)i / (float)(numProjectiles - 1) * inaccuracy;
					Vector2 direction = directionToCursor.Rotate(degrees);
					ThrowProjectileTowards(direction);
				}
			}
			else
			{
				Vector2 direction2 = directionToCursor.Rotate(UnityEngine.Random.Range(num, max));
				ThrowProjectileTowards(direction2);
			}
		}

		private Vector2 GetDirectionToCursor()
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 vector2 = PlayerController.Instance.transform.position;
			return vector - vector2;
		}

		private void ThrowProjectileTowards(Vector2 direction)
		{
			GameObject pooledObject = OP.GetPooledObject(projectilePrefab.name);
			pooledObject.SetActive(value: true);
			Projectile component = pooledObject.GetComponent<Projectile>();
			component.vector = speed * direction.normalized;
			if (!lockRotation)
			{
				component.angle = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
			}
			Vector2 vector = direction.normalized * spawnOffset;
			pooledObject.transform.position = player.transform.position + new Vector3(vector.x, vector.y, 0f);
		}
	}
}
