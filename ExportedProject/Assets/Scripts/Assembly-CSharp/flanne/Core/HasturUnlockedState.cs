namespace flanne.Core
{
	public class HasturUnlockedState : GameState
	{
		private void OnConfirm()
		{
			owner.ChangeState<PlayerSurvivedState>();
		}

		public override void Enter()
		{
			base.hasturUnlockedPanel.Show();
			base.hasturUnlockedConfirmButton.onClick.AddListener(OnConfirm);
			SaveSystem.data.characterUnlocks.unlocks[8] = true;
		}

		public override void Exit()
		{
			base.hasturUnlockedPanel.Hide();
			base.hasturUnlockedConfirmButton.onClick.RemoveListener(OnConfirm);
		}
	}
}
