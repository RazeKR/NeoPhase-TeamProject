using UnityEngine;

namespace flanne.PowerupSystem
{
	public class IcicleOnLastAmmo : MonoBehaviour
	{
		[SerializeField]
		private GameObject iciclePrefab;

		[SerializeField]
		private SoundEffectSO soundFX;

		[SerializeField]
		private int numIcicles;

		private ObjectPooler OP;

		private PlayerController player;

		private Ammo ammo;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(iciclePrefab.name, iciclePrefab, 10);
			player = GetComponentInParent<PlayerController>();
			ammo = player.ammo;
			ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
		}

		private void OnDestroy()
		{
			ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
		}

		private void OnAmmoChanged(int ammoAmount)
		{
			if (ammoAmount == 0)
			{
				SpawnIcicle();
			}
		}

		private void SpawnIcicle()
		{
			for (int i = 0; i < numIcicles; i++)
			{
				GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject(iciclePrefab.name);
				pooledObject.transform.position = player.transform.position;
				SeekEnemy component = pooledObject.GetComponent<SeekEnemy>();
				if (component != null)
				{
					component.player = player.transform;
				}
				MoveComponent2D component2 = pooledObject.GetComponent<MoveComponent2D>();
				if (component2 != null)
				{
					component2.vector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
				}
				pooledObject.SetActive(value: true);
			}
			soundFX?.Play();
		}
	}
}
