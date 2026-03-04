using UnityEngine;

namespace flanne.PowerupSystem
{
	public class SpawnTornadoOnShoot : AttackOnShoot
	{
		[SerializeField]
		private int numTornadoes;

		[SerializeField]
		private GameObject tornadoPrefab;

		[SerializeField]
		private float tornadoSpeed = 20f;

		[SerializeField]
		private float inaccuracy = 65f;

		[SerializeField]
		private bool fireBackwards;

		private ShootingCursor SC;

		private ObjectPooler OP;

		protected override void Init()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(tornadoPrefab.name, tornadoPrefab, 100);
			SC = ShootingCursor.Instance;
		}

		public override void Attack()
		{
			for (int i = 0; i < numTornadoes; i++)
			{
				SpawnTornado();
			}
		}

		private void SpawnTornado()
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 vector2 = base.transform.position;
			Vector2 normalized = (vector - vector2).normalized;
			GameObject pooledObject = OP.GetPooledObject(tornadoPrefab.name);
			pooledObject.SetActive(value: true);
			Vector2 vector3 = normalized.normalized * 1.2f;
			if (fireBackwards)
			{
				vector3 *= -1f;
			}
			pooledObject.transform.position = base.transform.position + new Vector3(vector3.x, vector3.y, 0f);
			float num = -1f * inaccuracy / 2f;
			float max = -1f * num;
			Vector2 vector4 = normalized.Rotate(Random.Range(num, max));
			if (fireBackwards)
			{
				vector4 *= -1f;
			}
			pooledObject.GetComponent<MoveComponent2D>().vector = tornadoSpeed * vector4;
		}
	}
}
