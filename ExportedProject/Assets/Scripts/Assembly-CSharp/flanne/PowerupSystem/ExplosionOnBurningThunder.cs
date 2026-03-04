using UnityEngine;

namespace flanne.PowerupSystem
{
	public class ExplosionOnBurningThunder : MonoBehaviour
	{
		[SerializeField]
		private GameObject explosionPrefab;

		[SerializeField]
		private ExplosionShake2D cameraShaker;

		[SerializeField]
		private SoundEffectSO soundFX;

		private ObjectPooler OP;

		private BurnSystem BurnSys;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(explosionPrefab.name, explosionPrefab, 5);
			BurnSys = BurnSystem.SharedInstance;
			this.AddObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnThunderHit(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			if (BurnSys.IsBurning(gameObject))
			{
				GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject(explosionPrefab.name);
				pooledObject.transform.position = gameObject.transform.position;
				pooledObject.SetActive(value: true);
				cameraShaker.Shake();
				soundFX?.Play();
			}
		}
	}
}
