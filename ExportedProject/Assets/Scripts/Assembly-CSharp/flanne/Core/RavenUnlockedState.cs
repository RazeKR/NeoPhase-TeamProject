namespace flanne.Core
{
	public class RavenUnlockedState : GameState
	{
		private void OnConfirm()
		{
			owner.ChangeState<PlayerSurvivedState>();
		}

		public override void Enter()
		{
			base.ravenUnlockedPanel.Show();
			base.ravenUnlockedConfirmButton.onClick.AddListener(OnConfirm);
			SaveSystem.data.characterUnlocks.unlocks[9] = true;
		}

		public override void Exit()
		{
			base.ravenUnlockedPanel.Hide();
			base.ravenUnlockedConfirmButton.onClick.RemoveListener(OnConfirm);
		}
	}
}
