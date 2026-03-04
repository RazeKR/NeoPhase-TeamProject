namespace flanne.PerkSystem.Triggers
{
	public class OnSHPLostTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			player.playerHealth.onLostSHP.AddListener(OnSHPLost);
		}

		public override void OnUnEquip(PlayerController player)
		{
			player.playerHealth.onLostSHP.RemoveListener(OnSHPLost);
		}

		private void OnSHPLost()
		{
			RaiseTrigger(PlayerController.Instance.gameObject);
		}
	}
}
