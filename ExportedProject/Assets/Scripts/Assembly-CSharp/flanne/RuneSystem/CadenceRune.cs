using UnityEngine;

namespace flanne.RuneSystem
{
	public class CadenceRune : Rune
	{
		[SerializeField]
		private float bonusStatsPerLevel;

		[SerializeField]
		private int shotsPerBuff;

		private int _counter;

		protected override void Init()
		{
			player.gun.OnShoot.AddListener(IncrementCounter);
		}

		private void OnDestroy()
		{
			player.gun.OnShoot.RemoveListener(IncrementCounter);
		}

		public void IncrementCounter()
		{
			_counter++;
			if (_counter == shotsPerBuff - 1)
			{
				Activate();
			}
			else if (_counter >= shotsPerBuff)
			{
				_counter = 0;
				Deactivate();
			}
		}

		private void Activate()
		{
			player.stats[StatType.Piercing].AddFlatBonus(99);
			float value = bonusStatsPerLevel * (float)level;
			player.stats[StatType.ProjectileSize].AddMultiplierBonus(value);
			player.stats[StatType.BulletDamage].AddMultiplierBonus(value);
		}

		private void Deactivate()
		{
			player.stats[StatType.Piercing].AddFlatBonus(-99);
			float num = bonusStatsPerLevel * (float)level;
			player.stats[StatType.ProjectileSize].AddMultiplierBonus(-1f * num);
			player.stats[StatType.BulletDamage].AddMultiplierBonus(-1f * num);
		}
	}
}
