using UnityEngine;

namespace flanne
{
	public class SpawnPrefabFromObjectPool : MonoBehaviour
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int amountToInitInObjPool;

		private bool isInitialized;

		private ObjectPooler OP;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(prefab.name, prefab, amountToInitInObjPool);
			isInitialized = true;
		}

		public void Spawn()
		{
			if (isInitialized)
			{
				GameObject pooledObject = OP.GetPooledObject(prefab.name);
				pooledObject.transform.position = base.transform.position;
				pooledObject.SetActive(value: true);
			}
		}
	}
}
