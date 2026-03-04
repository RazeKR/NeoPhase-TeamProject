using UnityEngine;

namespace flanne.PowerupSystem
{
	public class SummonOnEnemyDeath : MonoBehaviour
	{
		[SerializeField]
		private GameObject summonPrefab;

		[SerializeField]
		private SoundEffectSO soundFX;

		private ObjectPooler OP;

		private PlayerController player;

		private void Start()
		{
			this.AddObserver(OnDeath, Health.DeathEvent);
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(summonPrefab.name, summonPrefab, 200);
			player = GetComponentInParent<PlayerController>();
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDeath(object sender, object args)
		{
			GameObject gameObject = (sender as Health).gameObject;
			if (gameObject.tag == "Enemy")
			{
				GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject(summonPrefab.name);
				pooledObject.transform.SetParent(player.transform);
				pooledObject.transform.position = gameObject.transform.position;
				pooledObject.SetActive(value: true);
				soundFX?.Play();
			}
		}
	}
}
