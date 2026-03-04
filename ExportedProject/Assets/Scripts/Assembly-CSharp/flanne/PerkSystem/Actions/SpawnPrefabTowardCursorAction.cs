using System;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class SpawnPrefabTowardCursorAction : Action
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int amountToSpawn = 1;

		[SerializeField]
		private float distance;

		[SerializeField]
		private bool randomizeRotation;

		[SerializeField]
		private float maxAngle;

		[SerializeField]
		private float minAngle;

		[NonSerialized]
		private ObjectPooler OP;

		private PlayerController player;

		private ShootingCursor SC;

		public override void Init()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(prefab.name, prefab, 30);
			player = PlayerController.Instance;
			SC = ShootingCursor.Instance;
		}

		public override void Activate(GameObject target)
		{
			for (int i = 0; i < amountToSpawn; i++)
			{
				GameObject pooledObject = OP.GetPooledObject(prefab.name);
				Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
				Vector2 vector2 = player.transform.position;
				Vector2 vector3 = vector - vector2;
				Vector2 vector4 = vector2 + vector3 * distance;
				pooledObject.transform.position = vector4;
				if (randomizeRotation)
				{
					pooledObject.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(minAngle, maxAngle));
				}
				pooledObject.SetActive(value: true);
			}
		}
	}
}
