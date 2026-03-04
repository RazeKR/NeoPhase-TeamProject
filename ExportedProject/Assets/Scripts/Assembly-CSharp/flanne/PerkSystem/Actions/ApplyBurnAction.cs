using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ApplyBurnAction : Action
	{
		[SerializeField]
		private int burnDamage;

		public override void Activate(GameObject target)
		{
			BurnSystem.SharedInstance.Burn(target, burnDamage);
		}
	}
}
