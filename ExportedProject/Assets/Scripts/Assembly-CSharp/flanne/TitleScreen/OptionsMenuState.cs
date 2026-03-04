using flanne.UI;

namespace flanne.TitleScreen
{
	public class OptionsMenuState : TitleScreenState
	{
		public void OnClick(int i)
		{
			if (i == 0)
			{
				owner.ChangeState<TitleMainMenuState>();
			}
		}

		public void OnCancel()
		{
			owner.ChangeState<TitleMainMenuState>();
		}

		public override void Enter()
		{
			base.optionsMenu.GetComponent<OptionsSetter>().Refresh();
			base.optionsMenu.Show();
			base.optionsMenu.onClick.AddListener(OnClick);
			base.optionsMenu.onCancel.AddListener(OnCancel);
		}

		public override void Exit()
		{
			base.optionsMenu.Hide();
			base.optionsMenu.onClick.RemoveListener(OnClick);
			base.optionsMenu.onCancel.RemoveListener(OnCancel);
		}
	}
}
