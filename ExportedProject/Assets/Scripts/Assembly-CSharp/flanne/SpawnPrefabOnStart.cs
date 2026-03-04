using UnityEngine;

namespace flanne
{
	public class SpawnPrefabOnStart : MonoBehaviour
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int amountToInitInObjPool;

		[SerializeField]
		private int amountToSpawn;

		private ObjectPooler OP;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(prefab.name, prefab, amountToInitInObjPool);
			for (int i = 0; i < amountToSpawn; i++)
			{
				Spawn();
			}
		}

		public void Spawn()
		{
			GameObject pooledObject = OP.GetPooledObject(prefab.name);
			pooledObject.transform.position = base.transform.position;
			pooledObject.SetActive(value: true);
		}
	}
}
