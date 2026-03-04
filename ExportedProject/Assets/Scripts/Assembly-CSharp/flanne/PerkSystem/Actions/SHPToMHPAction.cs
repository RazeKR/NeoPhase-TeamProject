using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class SHPToMHPAction : Action
	{
		public override void Activate(GameObject target)
		{
			PlayerController instance = PlayerController.Instance;
			PlayerHealth playerHealth = instance.playerHealth;
			StatsHolder stats = instance.stats;
			if (playerHealth.shp > 0)
			{
				playerHealth.shp--;
				stats[StatType.MaxHP].AddFlatBonus(1);
			}
		}
	}
}
