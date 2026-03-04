using UnityEngine;

namespace flanne.RuneSystem
{
	public class SpawnerRune : Rune
	{
		[SerializeField]
		private GameObject spawnPrefab;

		[SerializeField]
		private float cooldownReductionPerLevel;

		[SerializeField]
		private float baseCooldown;

		[SerializeField]
		private SoundEffectSO soundFX;

		private float _timer;

		private ObjectPooler OP;

		private float cooldown => player.stats[StatType.SummonAttackSpeed].ModifyInverse(baseCooldown - cooldownReductionPerLevel * (float)level);

		protected override void Init()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(spawnPrefab.name, spawnPrefab, 10);
		}

		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer > cooldown)
			{
				_timer -= cooldown;
				GameObject pooledObject = OP.GetPooledObject(spawnPrefab.name);
				pooledObject.transform.position = base.transform.position;
				Spawn component = pooledObject.GetComponent<Spawn>();
				if (component != null)
				{
					component.player = player;
				}
				pooledObject.SetActive(value: true);
				soundFX?.Play();
			}
		}
	}
}
