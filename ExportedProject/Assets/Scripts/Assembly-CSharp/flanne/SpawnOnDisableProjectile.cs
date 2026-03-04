using UnityEngine;

namespace flanne
{
	public class SpawnOnDisableProjectile : Projectile
	{
		[SerializeField]
		private GameObject prefab;

		private bool isInitialized;

		private ObjectPooler OP;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(prefab.name, prefab, 100);
			isInitialized = true;
		}

		private void OnDisable()
		{
			if (!isSecondary && isInitialized)
			{
				GameObject pooledObject = OP.GetPooledObject(prefab.name);
				pooledObject.transform.position = base.transform.position;
				pooledObject.SetActive(value: true);
			}
		}
	}
}
