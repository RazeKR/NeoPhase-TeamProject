namespace flanne.TitleScreen
{
	public class LangaugeState : TitleScreenState
	{
		public void OnClick(int i)
		{
			owner.ChangeState<TitleMainMenuState>();
		}

		public void OnCancel()
		{
			owner.ChangeState<TitleMainMenuState>();
		}

		public override void Enter()
		{
			base.logoPanel.Hide();
			base.languageMenu.Show();
			base.languageMenu.onClick.AddListener(OnClick);
			base.languageMenu.onCancel.AddListener(OnCancel);
		}

		public override void Exit()
		{
			base.languageMenu.Hide();
			base.languageMenu.onClick.RemoveListener(OnClick);
			base.languageMenu.onCancel.RemoveListener(OnCancel);
		}
	}
}
