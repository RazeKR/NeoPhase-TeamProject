namespace flanne.PerkSystem.Triggers
{
	public class InstantTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			RaiseTrigger(player.gameObject);
		}
	}
}
