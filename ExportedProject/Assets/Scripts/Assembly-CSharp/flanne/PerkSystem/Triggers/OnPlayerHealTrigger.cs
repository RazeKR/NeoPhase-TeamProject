namespace flanne.PerkSystem.Triggers
{
	public class OnPlayerHealTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			player.playerHealth.onHealed.AddListener(OnHeal);
		}

		public override void OnUnEquip(PlayerController player)
		{
			player.playerHealth.onHealed.RemoveListener(OnHeal);
		}

		private void OnHeal()
		{
			RaiseTrigger(PlayerController.Instance.gameObject);
		}
	}
}
