using UnityEngine;

namespace flanne.RuneSystem
{
	public class SpreadStatusRune : Rune
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float spreadChancePerLevel;

		[SerializeField]
		private GameObject burnSpreadPrefab;

		[SerializeField]
		private GameObject freezeSpreadPrefab;

		private BurnSystem BurnSys;

		private FreezeSystem FreezeSys;

		private ObjectPooler OP;

		private void OnDeath(object sender, object args)
		{
			if (!(Random.Range(0f, 1f) > spreadChancePerLevel * (float)level))
			{
				GameObject gameObject = (sender as Health).gameObject;
				if (BurnSys.IsBurning(gameObject))
				{
					GameObject pooledObject = OP.GetPooledObject(burnSpreadPrefab.name);
					pooledObject.transform.position = gameObject.transform.position;
					pooledObject.SetActive(value: true);
				}
				if (FreezeSys.IsFrozen(gameObject))
				{
					GameObject pooledObject2 = OP.GetPooledObject(freezeSpreadPrefab.name);
					pooledObject2.transform.position = gameObject.transform.position;
					pooledObject2.SetActive(value: true);
				}
			}
		}

		private void Start()
		{
			BurnSys = BurnSystem.SharedInstance;
			FreezeSys = FreezeSystem.SharedInstance;
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(burnSpreadPrefab.name, burnSpreadPrefab, 100);
			OP.AddObject(freezeSpreadPrefab.name, freezeSpreadPrefab, 100);
			this.AddObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnDeath, Health.DeathEvent);
		}
	}
}
