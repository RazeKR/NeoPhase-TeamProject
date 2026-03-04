namespace flanne.TitleScreen
{
	public class ModeSelectState : TitleScreenState
	{
		public void OnClickPlay()
		{
			if (base.modeSelectMenu.currIndex == 0)
			{
				owner.ChangeState<MapSelectState>();
				return;
			}
			SelectedMap.MapData = base.modeSelectMenu.toggledData;
			owner.ChangeState<WaitToLoadIntoBattleState>();
			base.selectPanel.Hide();
			base.gunMenu.Hide();
		}

		public void OnClickBack()
		{
			owner.ChangeState<GunSelectState>();
		}

		public override void Enter()
		{
			base.modeSelectPanel.Show();
			base.modeSelectMenu.RefreshDescription();
			base.modeSelectMenu.RefreshToggleData();
			base.difficultyController.RefreshText();
			base.modeSelectStartButton.onClick.AddListener(OnClickPlay);
			base.modeSelectBackButton.onClick.AddListener(OnClickBack);
			base.modeSelectStartButton.Select();
		}

		public override void Exit()
		{
			base.modeSelectPanel.Hide();
			base.modeSelectStartButton.onClick.RemoveListener(OnClickPlay);
			base.modeSelectBackButton.onClick.RemoveListener(OnClickBack);
		}
	}
}
