namespace flanne.Core
{
	public class PauseState : GameState
	{
		private void OnResume()
		{
			owner.ChangeState<CombatState>();
			base.pauseController.UnPause();
			base.powerupListUI.Hide();
			AudioManager.Instance.SetLowPassFilter(isOn: false);
		}

		private void OnOptions()
		{
			owner.ChangeState<OptionsState>();
		}

		private void OnSynergies()
		{
			owner.ChangeState<SynergyUIState>();
		}

		private void OnGiveUp()
		{
			owner.ChangeState<CombatState>();
			base.playerHealth.AutoKill();
			base.pauseController.UnPause();
			base.powerupListUI.Hide();
			AudioManager.Instance.SetLowPassFilter(isOn: false);
		}

		public override void Enter()
		{
			AudioManager.Instance.SetLowPassFilter(isOn: true);
			base.pauseMenu.Show();
			base.powerupListUI.Show();
			base.pauseResumeButton.onClick.AddListener(OnResume);
			base.optionsButton.onClick.AddListener(OnOptions);
			base.synergiesButton.onClick.AddListener(OnSynergies);
			base.giveupButton.onClick.AddListener(OnGiveUp);
		}

		public override void Exit()
		{
			base.pauseMenu.Hide();
			base.pauseResumeButton.onClick.RemoveListener(OnResume);
			base.optionsButton.onClick.RemoveListener(OnOptions);
			base.synergiesButton.onClick.RemoveListener(OnSynergies);
			base.giveupButton.onClick.RemoveListener(OnGiveUp);
		}
	}
}
