using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using flanne.UI;

namespace flanne.Core
{
	public class ChestState : GameState
	{
		private Powerup _currPowerup;

		private void OnTakeClick(object sender, EventArgs e)
		{
			PlayerController.Instance.playerPerks.Equip(_currPowerup);
			base.powerupGenerator.RemoveFromCharacterPool(_currPowerup);
			StartCoroutine(WaitToLeaveMenu());
		}

		private void OnLeaveClick(object sender, EventArgs e)
		{
			StartCoroutine(WaitToLeaveMenu());
		}

		public override void Enter()
		{
			List<Powerup> randomCharacterProfile = base.powerupGenerator.GetRandomCharacterProfile();
			_currPowerup = randomCharacterProfile[0];
			base.chestUIController.SetToPowerup(_currPowerup);
			base.chestUIController.Show();
			ChestUIController obj = base.chestUIController;
			obj.TakeClickEvent = (EventHandler)Delegate.Combine(obj.TakeClickEvent, new EventHandler(OnTakeClick));
			ChestUIController obj2 = base.chestUIController;
			obj2.LeaveClickEvent = (EventHandler)Delegate.Combine(obj2.LeaveClickEvent, new EventHandler(OnLeaveClick));
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
			ChestUIController obj = base.chestUIController;
			obj.TakeClickEvent = (EventHandler)Delegate.Remove(obj.TakeClickEvent, new EventHandler(OnTakeClick));
			ChestUIController obj2 = base.chestUIController;
			obj2.LeaveClickEvent = (EventHandler)Delegate.Remove(obj2.LeaveClickEvent, new EventHandler(OnLeaveClick));
			base.chestUIController.Hide();
			yield return new WaitForSecondsRealtime(0.5f);
			if (base.playerHealth.hp != 0)
			{
				owner.ChangeState<CombatState>();
			}
			else
			{
				owner.ChangeState<PlayerDeadState>();
			}
		}
	}
}
