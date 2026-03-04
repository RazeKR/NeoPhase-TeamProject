using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnAmmoRemainTrigger : Trigger
	{
		[SerializeField]
		private int ammoAmount;

		public override void OnEquip(PlayerController player)
		{
			player.ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
		}

		public override void OnUnEquip(PlayerController player)
		{
			player.ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
		}

		private void OnAmmoChanged(int a)
		{
			if (a == ammoAmount)
			{
				RaiseTrigger(PlayerController.Instance.gameObject);
			}
		}
	}
}
