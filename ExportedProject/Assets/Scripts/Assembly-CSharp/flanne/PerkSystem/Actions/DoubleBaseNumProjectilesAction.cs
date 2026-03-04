using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class DoubleBaseNumProjectilesAction : Action
	{
		public override void Activate(GameObject target)
		{
			PlayerController instance = PlayerController.Instance;
			instance.stats[StatType.Projectiles].AddFlatBonus(instance.gun.gunData.numOfProjectiles);
		}
	}
}
