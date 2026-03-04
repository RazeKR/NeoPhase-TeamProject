using UnityEngine;

namespace flanne
{
	public class SpawnSummonOnReload : MonoBehaviour
	{
		[SerializeField]
		private GameObject summonPrefab;

		[SerializeField]
		private float spawnDistanceAway;

		[SerializeField]
		private SoundEffectSO soundFX;

		private float _timer;

		private ObjectPooler OP;

		private Ammo ammo;

		private void OnReload()
		{
			Spawn();
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(summonPrefab.name, summonPrefab, 30);
			ammo = PlayerController.Instance.ammo;
			ammo.OnReload.AddListener(OnReload);
		}

		private void OnDestroy()
		{
			ammo.OnReload.RemoveListener(OnReload);
		}

		private void Spawn()
		{
			GameObject pooledObject = OP.GetPooledObject(summonPrefab.name);
			Vector3 normalized = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
			Vector3 position = base.transform.position + normalized * spawnDistanceAway;
			pooledObject.transform.position = position;
			pooledObject.GetComponent<Spawn>();
			pooledObject.SetActive(value: true);
			soundFX?.Play();
		}
	}
}
