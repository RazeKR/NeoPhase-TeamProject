namespace flanne.TitleScreen
{
	public class MapSelectState : TitleScreenState
	{
		public void OnClickPlay()
		{
			SelectedMap.MapData = base.mapSelectMenu.toggledData;
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
			base.mapSelectPanel.Show();
			base.mapSelectStartButton.onClick.AddListener(OnClickPlay);
			base.mapSelectBackButton.onClick.AddListener(OnClickBack);
			base.mapSelectStartButton.Select();
		}

		public override void Exit()
		{
			base.mapSelectPanel.Hide();
			base.mapSelectStartButton.onClick.RemoveListener(OnClickPlay);
			base.mapSelectBackButton.onClick.RemoveListener(OnClickBack);
		}
	}
}
