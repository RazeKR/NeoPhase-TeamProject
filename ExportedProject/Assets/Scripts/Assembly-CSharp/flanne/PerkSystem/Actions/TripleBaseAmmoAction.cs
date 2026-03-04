using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class TripleBaseAmmoAction : Action
	{
		public override void Activate(GameObject target)
		{
			PlayerController instance = PlayerController.Instance;
			instance.stats[StatType.MaxAmmo].AddFlatBonus(2 * instance.gun.gunData.maxAmmo);
		}
	}
}
