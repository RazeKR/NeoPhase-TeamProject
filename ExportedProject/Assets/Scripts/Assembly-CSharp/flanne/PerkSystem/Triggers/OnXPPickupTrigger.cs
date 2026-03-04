using UnityEngine;
using flanne.Pickups;

namespace flanne.PerkSystem.Triggers
{
	public class OnXPPickupTrigger : Trigger
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToActivate;

		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnXPPickup, XPPickup.XPPickupEvent);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnXPPickup, XPPickup.XPPickupEvent);
		}

		private void OnXPPickup(object sender, object args)
		{
			if (Random.Range(0f, 1f) < chanceToActivate)
			{
				RaiseTrigger(PlayerController.Instance.gameObject);
			}
		}
	}
}
