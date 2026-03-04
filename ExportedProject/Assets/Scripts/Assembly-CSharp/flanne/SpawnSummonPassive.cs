using UnityEngine;

namespace flanne
{
	public class SpawnSummonPassive : MonoBehaviour
	{
		[SerializeField]
		private GameObject summonPrefab;

		[SerializeField]
		private float spawnDistanceAway;

		[SerializeField]
		private float cooldown;

		[SerializeField]
		private SoundEffectSO soundFX;

		private float _timer;

		private ObjectPooler OP;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(summonPrefab.name, summonPrefab, 30);
		}

		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer > cooldown)
			{
				_timer -= cooldown;
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
}
