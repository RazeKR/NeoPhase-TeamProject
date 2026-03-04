using System.Collections.Generic;

namespace flanne.Core
{
	public class DevilDealState : GameState
	{
		private List<Powerup> powerupChoices;

		private void OnConfirm(object sender, Powerup e)
		{
			PlayerController.Instance.playerPerks.Equip(e);
			base.powerupGenerator.RemoveFromDevilPool(e);
			if (base.playerHealth.hp != 0)
			{
				owner.ChangeState<CombatState>();
			}
			else
			{
				owner.ChangeState<PlayerDeadState>();
			}
		}

		private void OnLeave()
		{
			owner.ChangeState<CombatState>();
		}

		public override void Enter()
		{
			base.pauseController.Pause();
			GeneratePowerups();
			base.devilDealMenuPanel.Show();
			base.devilDealMenu.ConfirmEvent += OnConfirm;
			base.devilDealLeaveButton.onClick.AddListener(OnLeave);
			AudioManager.Instance.SetLowPassFilter(isOn: true);
		}

		public override void Exit()
		{
			base.pauseController.UnPause();
			base.devilDealMenuPanel.Hide();
			base.devilDealMenu.ConfirmEvent -= OnConfirm;
			base.devilDealLeaveButton.onClick.RemoveListener(OnLeave);
			AudioManager.Instance.SetLowPassFilter(isOn: false);
		}

		private void GeneratePowerups()
		{
			powerupChoices = base.powerupGenerator.GetRandomDevilProfile(3);
			for (int i = 0; i < powerupChoices.Count; i++)
			{
				base.devilDealMenu.SetData(i, powerupChoices[i]);
			}
		}
	}
}
