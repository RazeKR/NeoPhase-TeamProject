using UnityEngine;

namespace flanne.RuneSystem
{
	public class HPThresholdStatRune : Rune
	{
		[SerializeField]
		private StatChange[] statChanges = new StatChange[0];

		private bool statsActive;

		private PlayerHealth playerHealth;

		private void OnHpChange(int hp)
		{
			if (statsActive != playerHealth.hp <= playerHealth.maxHP / 2)
			{
				if (playerHealth.hp <= playerHealth.maxHP / 2)
				{
					Activate();
				}
				else
				{
					Deactivate();
				}
			}
		}

		protected override void Init()
		{
			statsActive = false;
			playerHealth = player.playerHealth;
			playerHealth.onHealthChangedTo.AddListener(OnHpChange);
		}

		private void OnDestroy()
		{
			playerHealth.onHealthChangedTo.RemoveListener(OnHpChange);
		}

		private void Activate()
		{
			StatChange[] array = statChanges;
			foreach (StatChange s in array)
			{
				for (int j = 0; j < level; j++)
				{
					ApplyStat(s);
				}
			}
			statsActive = true;
		}

		private void Deactivate()
		{
			StatChange[] array = statChanges;
			foreach (StatChange s in array)
			{
				for (int j = 0; j < level; j++)
				{
					RemoveStat(s);
				}
			}
			statsActive = false;
		}

		private void ApplyStat(StatChange s)
		{
			StatsHolder stats = player.stats;
			if (s.isFlatMod)
			{
				stats[s.type].AddFlatBonus(s.flatValue);
			}
			else if (s.value > 0f)
			{
				stats[s.type].AddMultiplierBonus(s.value);
			}
			else if (s.value < 0f)
			{
				stats[s.type].AddMultiplierReduction(1f + s.value);
			}
			if (s.type == StatType.MaxHP)
			{
				player.playerHealth.maxHP = Mathf.FloorToInt(stats[s.type].Modify(player.playerHealth.maxHP));
			}
			if (s.type == StatType.CharacterSize)
			{
				player.playerSprite.transform.localScale = Vector3.one * stats[s.type].Modify(1f);
			}
			if (s.type == StatType.PickupRange)
			{
				GameObject.FindGameObjectWithTag("Pickupper").transform.localScale = Vector3.one * stats[s.type].Modify(1f);
			}
			if (s.type == StatType.VisionRange)
			{
				GameObject.FindGameObjectWithTag("PlayerVision").transform.localScale = Vector3.one * stats[s.type].Modify(1f);
			}
		}

		private void RemoveStat(StatChange s)
		{
			StatsHolder stats = player.stats;
			if (s.isFlatMod)
			{
				stats[s.type].AddFlatBonus(-1 * s.flatValue);
			}
			else if (s.value > 0f)
			{
				stats[s.type].AddMultiplierBonus(-1f * s.value);
			}
			else if (s.value < 0f)
			{
				stats[s.type].AddMultiplierReduction(1f + -1f * s.value);
			}
			if (s.type == StatType.MaxHP)
			{
				player.playerHealth.maxHP = Mathf.FloorToInt(stats[s.type].Modify(player.playerHealth.maxHP));
			}
			if (s.type == StatType.CharacterSize)
			{
				player.playerSprite.transform.localScale = Vector3.one * stats[s.type].Modify(1f);
			}
			if (s.type == StatType.PickupRange)
			{
				GameObject.FindGameObjectWithTag("Pickupper").transform.localScale = Vector3.one * stats[s.type].Modify(1f);
			}
			if (s.type == StatType.VisionRange)
			{
				GameObject.FindGameObjectWithTag("PlayerVision").transform.localScale = Vector3.one * stats[s.type].Modify(1f);
			}
		}
	}
}
