using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class RefillAmmoAction : Action
	{
		[SerializeField]
		private int refillAmount;

		public override void Activate(GameObject target)
		{
			PlayerController.Instance.ammo.GainAmmo(refillAmount);
		}
	}
}
