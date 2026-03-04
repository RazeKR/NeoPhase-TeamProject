using UnityEngine;

namespace flanne.Pickups
{
	public class HPPickup : Pickup
	{
		public int amount;

		protected override void UsePickup(GameObject pickupper)
		{
			PlayerHealth componentInChildren = pickupper.transform.root.GetComponentInChildren<PlayerHealth>();
			if (componentInChildren != null)
			{
				componentInChildren.Heal(amount);
			}
		}
	}
}
