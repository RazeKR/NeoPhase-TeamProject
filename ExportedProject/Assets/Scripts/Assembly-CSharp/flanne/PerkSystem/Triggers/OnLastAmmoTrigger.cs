namespace flanne.PerkSystem.Triggers
{
	public class OnLastAmmoTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			player.ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
		}

		public override void OnUnEquip(PlayerController player)
		{
			player.ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
		}

		private void OnAmmoChanged(int ammoAmount)
		{
			if (ammoAmount == 0)
			{
				RaiseTrigger(PlayerController.Instance.gameObject);
			}
		}
	}
}
