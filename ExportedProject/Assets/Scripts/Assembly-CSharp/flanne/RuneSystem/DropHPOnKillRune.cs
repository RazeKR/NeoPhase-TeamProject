using UnityEngine;

namespace flanne.RuneSystem
{
	public class DropHPOnKillRune : Rune
	{
		[SerializeField]
		private GameObject hpPrefab;

		[SerializeField]
		private int baseKillThreshold;

		[SerializeField]
		private int thresholdReductionPerLevel;

		private int _counter;

		private ObjectPooler OP;

		public int killThreshold => baseKillThreshold - thresholdReductionPerLevel * level;

		protected override void Init()
		{
			this.AddObserver(OnDeath, Health.DeathEvent);
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(hpPrefab.name, hpPrefab, 100);
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
				_counter++;
				if (_counter >= killThreshold)
				{
					DropHP(gameObject.transform.position);
					_counter = 0;
				}
			}
		}

		private void DropHP(Vector3 position)
		{
			GameObject pooledObject = OP.GetPooledObject(hpPrefab.name);
			pooledObject.transform.position = position;
			pooledObject.SetActive(value: true);
		}
	}
}
