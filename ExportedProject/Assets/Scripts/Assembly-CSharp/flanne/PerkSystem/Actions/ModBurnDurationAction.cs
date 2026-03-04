using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModBurnDurationAction : Action
	{
		[SerializeField]
		private float burnDurationMulti;

		public override void Activate(GameObject target)
		{
			BurnSystem.SharedInstance.burnDurationMultiplier.AddMultiplierBonus(burnDurationMulti);
		}
	}
}
