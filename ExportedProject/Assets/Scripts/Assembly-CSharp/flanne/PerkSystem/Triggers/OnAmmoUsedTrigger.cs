using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnAmmoUsedTrigger : Trigger
	{
		[SerializeField]
		private int ammoUsed;

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
			if (Mathf.Max(1, PlayerController.Instance.ammo.max) - a == ammoUsed)
			{
				RaiseTrigger(PlayerController.Instance.gameObject);
			}
		}
	}
}
