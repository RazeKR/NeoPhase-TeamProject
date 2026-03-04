using UnityEngine;

namespace flanne
{
	public class Spawner : MonoBehaviour
	{
		[SerializeField]
		private float initialSpeed;

		[SerializeField]
		private GameObject spawnedObject;

		[SerializeField]
		private Transform[] spawnPoints;

		private ObjectPooler OP;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(spawnedObject.name, spawnedObject, 100);
		}

		public void Spawn()
		{
			Transform[] array = spawnPoints;
			foreach (Transform transform in array)
			{
				GameObject pooledObject = OP.GetPooledObject(spawnedObject.name);
				pooledObject.transform.position = transform.position;
				pooledObject.SetActive(value: true);
				pooledObject.GetComponent<MoveComponent2D>().vector = (pooledObject.transform.position - base.transform.position).normalized * initialSpeed;
			}
		}
	}
}
