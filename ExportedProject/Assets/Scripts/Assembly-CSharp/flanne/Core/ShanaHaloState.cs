using System.Collections;
using UnityEngine;

namespace flanne.Core
{
	public class ShanaHaloState : GameState
	{
		private Powerup _currPowerup;

		private void OnTakeClick()
		{
			PlayerController.Instance.playerPerks.Equip(_currPowerup);
			StartCoroutine(WaitToLeaveMenu());
		}

		public override void Enter()
		{
			_currPowerup = base.haloUI.data;
			base.haloUIPanel.Show();
			base.takeHaloButton.onClick.AddListener(OnTakeClick);
			base.pauseController.Pause();
			AudioManager.Instance.SetLowPassFilter(isOn: true);
		}

		public override void Exit()
		{
			base.pauseController.UnPause();
			AudioManager.Instance.SetLowPassFilter(isOn: false);
		}

		private IEnumerator WaitToLeaveMenu()
		{
			base.takeHaloButton.onClick.RemoveListener(OnTakeClick);
			base.haloUIPanel.Hide();
			yield return new WaitForSecondsRealtime(0.5f);
			owner.ChangeState<CombatState>();
		}
	}
}
