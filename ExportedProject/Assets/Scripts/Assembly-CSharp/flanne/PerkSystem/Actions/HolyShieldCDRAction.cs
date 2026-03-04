using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class HolyShieldCDRAction : Action
	{
		[SerializeField]
		private float additionalCDR;

		public override void Activate(GameObject target)
		{
			PlayerController.Instance.GetComponentInChildren<PreventDamage>().cooldownRate.AddMultiplierBonus(additionalCDR);
		}
	}
}
