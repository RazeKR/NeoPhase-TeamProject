using UnityEngine;
using flanne.Player;

namespace flanne.Pickups
{
	public class XPPickup : Pickup
	{
		public static string XPPickupEvent = "XPPickup.XPPickupEvent";

		public float amount;

		protected override void UsePickup(GameObject pickupper)
		{
			PlayerXP componentInChildren = pickupper.transform.root.GetComponentInChildren<PlayerXP>();
			if (componentInChildren != null)
			{
				componentInChildren.GainXP(amount);
			}
			this.PostNotification(XPPickupEvent, null);
		}
	}
}
