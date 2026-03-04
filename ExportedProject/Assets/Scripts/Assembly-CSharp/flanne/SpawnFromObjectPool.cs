using UnityEngine;

namespace flanne
{
	public class SpawnFromObjectPool : MonoBehaviour
	{
		public void Spawn(string objectPoolTag)
		{
			GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject(objectPoolTag);
			pooledObject.transform.position = base.transform.position;
			pooledObject.SetActive(value: true);
		}
	}
}
