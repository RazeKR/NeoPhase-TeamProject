using UnityEngine;

namespace flanne
{
	public class DropPrefabOverTime : MonoBehaviour
	{
		[SerializeField]
		private float timeToDrop;

		[SerializeField]
		private GameObject prefab;

		private ObjectPooler OP;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(prefab.name, prefab, 5);
			InvokeRepeating("DropHP", 0f, timeToDrop);
		}

		private void OnDestroy()
		{
			CancelInvoke();
		}

		private void DropHP()
		{
			GameObject pooledObject = OP.GetPooledObject(prefab.name);
			pooledObject.transform.position = base.transform.position;
			pooledObject.SetActive(value: true);
		}
	}
}
