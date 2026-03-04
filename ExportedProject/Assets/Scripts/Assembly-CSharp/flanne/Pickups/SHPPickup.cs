using UnityEngine;

namespace flanne.Pickups
{
	public class SHPPickup : Pickup
	{
		public int amount;

		protected override void UsePickup(GameObject pickupper)
		{
			PlayerHealth componentInChildren = pickupper.transform.root.GetComponentInChildren<PlayerHealth>();
			if (componentInChildren != null)
			{
				componentInChildren.shp++;
			}
		}
	}
}
