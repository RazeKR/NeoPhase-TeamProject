using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModFreezeDurationAction : Action
	{
		[SerializeField]
		private float freezeDurationMutli;

		public override void Activate(GameObject target)
		{
			FreezeSystem.SharedInstance.durationMod.AddMultiplierBonus(freezeDurationMutli);
		}
	}
}
