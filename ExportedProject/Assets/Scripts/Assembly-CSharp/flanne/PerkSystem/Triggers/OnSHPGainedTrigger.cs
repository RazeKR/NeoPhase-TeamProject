namespace flanne.PerkSystem.Triggers
{
	public class OnSHPGainedTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			player.playerHealth.onGainedSHP.AddListener(OnSHPGained);
		}

		public override void OnUnEquip(PlayerController player)
		{
			player.playerHealth.onGainedSHP.RemoveListener(OnSHPGained);
		}

		private void OnSHPGained()
		{
			RaiseTrigger(PlayerController.Instance.gameObject);
		}
	}
}
