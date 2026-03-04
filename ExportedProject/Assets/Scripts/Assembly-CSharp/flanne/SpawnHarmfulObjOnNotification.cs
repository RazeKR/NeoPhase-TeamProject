using UnityEngine;

namespace flanne
{
	public class SpawnHarmfulObjOnNotification : MonoBehaviour
	{
		[SerializeField]
		private string notification;

		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int amountToInitInObjPool;

		[SerializeField]
		private float damageMultiplier;

		private ObjectPooler OP;

		private Gun gun;

		private void OnNotification(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			Spawn(gameObject.transform.position);
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(prefab.name, prefab, amountToInitInObjPool);
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			gun = componentInParent.gun;
			this.AddObserver(OnNotification, notification);
		}

		private void OnDisable()
		{
			this.RemoveObserver(OnNotification, notification);
		}

		public void Spawn(Vector3 spawnPos)
		{
			GameObject pooledObject = OP.GetPooledObject(prefab.name);
			pooledObject.transform.position = spawnPos;
			pooledObject.GetComponent<Harmful>().damageAmount = Mathf.FloorToInt(gun.damage * damageMultiplier);
			pooledObject.SetActive(value: true);
		}
	}
}
