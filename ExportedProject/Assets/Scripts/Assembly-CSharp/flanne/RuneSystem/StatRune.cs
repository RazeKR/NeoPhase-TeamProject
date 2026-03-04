using UnityEngine;

namespace flanne.RuneSystem
{
	public class StatRune : Rune
	{
		[SerializeField]
		private StatChange[] statChanges = new StatChange[0];

		protected override void Init()
		{
			StatChange[] array = statChanges;
			foreach (StatChange s in array)
			{
				for (int j = 0; j < level; j++)
				{
					ApplyStat(s);
				}
			}
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
	}
}
