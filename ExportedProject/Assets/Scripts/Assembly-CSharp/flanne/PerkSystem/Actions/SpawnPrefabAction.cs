using System;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class SpawnPrefabAction : Action
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int amountToSpawn = 1;

		[SerializeField]
		private bool randomizeRotation;

		[SerializeField]
		private float maxAngle;

		[SerializeField]
		private float minAngle;

		[NonSerialized]
		private ObjectPooler OP;

		public override void Init()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(prefab.name, prefab, 30);
		}

		public override void Activate(GameObject target)
		{
			for (int i = 0; i < amountToSpawn; i++)
			{
				GameObject pooledObject = OP.GetPooledObject(prefab.name);
				pooledObject.transform.position = target.transform.position;
				if (randomizeRotation)
				{
					pooledObject.transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(minAngle, maxAngle));
				}
				pooledObject.SetActive(value: true);
			}
		}
	}
}
