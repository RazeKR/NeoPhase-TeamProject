namespace flanne.Core
{
	public class SynergyUIState : GameState
	{
		private void OnBack()
		{
			owner.ChangeState<PauseState>();
		}

		public override void Enter()
		{
			base.synergiesUIPanel.Show();
			base.synergiesUIBackButton.onClick.AddListener(OnBack);
		}

		public override void Exit()
		{
			base.synergiesUIPanel.Hide();
			base.synergiesUIBackButton.onClick.RemoveListener(OnBack);
		}
	}
}
