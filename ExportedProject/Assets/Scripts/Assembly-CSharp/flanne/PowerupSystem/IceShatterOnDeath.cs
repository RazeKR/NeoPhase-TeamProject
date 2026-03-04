using UnityEngine;

namespace flanne.PowerupSystem
{
	public class IceShatterOnDeath : MonoBehaviour
	{
		[SerializeField]
		private GameObject shatterPrefab;

		[Range(0f, 1f)]
		[SerializeField]
		private float shatterPercentDamage;

		[SerializeField]
		private SoundEffectSO soundFX;

		private ObjectPooler OP;

		private FreezeSystem FreezeSys;

		private PlayerController player;

		private void Start()
		{
			this.AddObserver(OnDeath, Health.DeathEvent);
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(shatterPrefab.name, shatterPrefab, 25);
			FreezeSys = FreezeSystem.SharedInstance;
			player = GetComponentInParent<PlayerController>();
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDeath(object sender, object args)
		{
			Health health = sender as Health;
			GameObject gameObject = health.gameObject;
			if (gameObject.tag == "Enemy" && FreezeSys.IsFrozen(gameObject))
			{
				GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject(shatterPrefab.name);
				pooledObject.transform.position = gameObject.transform.position;
				pooledObject.GetComponent<Harmful>().damageAmount = Mathf.FloorToInt((float)health.maxHP * shatterPercentDamage);
				pooledObject.SetActive(value: true);
				soundFX?.Play();
			}
		}
	}
}
