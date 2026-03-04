namespace flanne.PerkSystem.Triggers
{
	public class OnReloadTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			PlayerController.Instance.ammo.OnReload.AddListener(OnReload);
		}

		public override void OnUnEquip(PlayerController player)
		{
			PlayerController.Instance.ammo.OnReload.AddListener(OnReload);
		}

		private void OnReload()
		{
			RaiseTrigger(PlayerController.Instance.gameObject);
		}
	}
}
