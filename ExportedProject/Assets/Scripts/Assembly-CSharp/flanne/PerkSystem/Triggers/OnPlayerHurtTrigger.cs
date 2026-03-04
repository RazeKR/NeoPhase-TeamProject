namespace flanne.PerkSystem.Triggers
{
	public class OnPlayerHurtTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			player.playerHealth.onHurt.AddListener(OnPlayerHurt);
		}

		public override void OnUnEquip(PlayerController player)
		{
			player.playerHealth.onHurt.RemoveListener(OnPlayerHurt);
		}

		private void OnPlayerHurt()
		{
			RaiseTrigger(PlayerController.Instance.gameObject);
		}
	}
}
