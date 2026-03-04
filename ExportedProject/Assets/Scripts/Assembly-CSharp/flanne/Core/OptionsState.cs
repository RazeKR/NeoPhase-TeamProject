namespace flanne.Core
{
	public class OptionsState : GameState
	{
		public void OnClick(int i)
		{
			if (i == 0)
			{
				owner.ChangeState<PauseState>();
			}
		}

		private void OnBack()
		{
			owner.ChangeState<PauseState>();
		}

		public override void Enter()
		{
			base.optionsMenu.Show();
			base.optionsMenu.onClick.AddListener(OnClick);
			base.optionsMenu.onCancel.AddListener(OnBack);
		}

		public override void Exit()
		{
			base.optionsMenu.Hide();
			base.optionsMenu.onClick.RemoveListener(OnClick);
			base.optionsMenu.onCancel.RemoveListener(OnBack);
		}
	}
}
