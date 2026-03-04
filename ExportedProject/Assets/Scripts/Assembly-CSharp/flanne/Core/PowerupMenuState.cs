using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace flanne.Core
{
	public class PowerupMenuState : GameState
	{
		private List<Powerup> powerupChoices;

		private void OnConfirm(object sender, Powerup e)
		{
			StartCoroutine(EndLevelUpAnimationCR());
			PlayerController.Instance.playerPerks.Equip(e);
			base.powerupGenerator.RemoveFromPool(e);
		}

		private void OnReroll()
		{
			GeneratePowerups();
			base.powerupRerollButton.gameObject.SetActive(value: false);
			base.powerupMenuPanel.SelectDefault();
		}

		public override void Enter()
		{
			base.pauseController.Pause();
			StartCoroutine(PlayLevelUpAnimationCR());
			AudioManager.Instance.SetLowPassFilter(isOn: true);
		}

		public override void Exit()
		{
			AudioManager.Instance.SetLowPassFilter(isOn: false);
		}

		private void GeneratePowerups()
		{
			int num = 5;
			powerupChoices = base.powerupGenerator.GetRandom(base.numPowerupChoices);
			for (int i = 0; i < base.numPowerupChoices; i++)
			{
				base.powerupMenu.SetData(i, powerupChoices[i]);
				base.powerupMenu.SetActive(i, isActive: true);
			}
			for (int j = base.numPowerupChoices; j < num; j++)
			{
				base.powerupMenu.SetActive(j, isActive: false);
			}
		}

		private IEnumerator PlayLevelUpAnimationCR()
		{
			base.screenFlash.Flash(1);
			base.levelupAnimator.gameObject.SetActive(value: true);
			base.levelupAnimator.SetTrigger("Start");
			LeanTween.scale(base.playerFogRevealer, new Vector3(2f, 2f, 1f), 0.7f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(useUnScaledTime: true);
			yield return new WaitForSecondsRealtime(0.6f);
			base.screenFlash.Flash(1);
			base.powerupMenuSFX.Play();
			yield return new WaitForSecondsRealtime(0.1f);
			GeneratePowerups();
			base.powerupMenuPanel.Show();
			base.powerupMenu.ConfirmEvent += OnConfirm;
			base.powerupRerollButton.onClick.AddListener(OnReroll);
			if (PowerupGenerator.CanReroll)
			{
				base.powerupRerollButton.gameObject.SetActive(value: true);
			}
		}

		private IEnumerator EndLevelUpAnimationCR()
		{
			base.levelupAnimator.SetTrigger("End");
			LeanTween.scale(base.playerFogRevealer, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutCubic).setIgnoreTimeScale(useUnScaledTime: true);
			base.powerupMenu.ConfirmEvent -= OnConfirm;
			base.powerupRerollButton.onClick.RemoveListener(OnReroll);
			base.powerupRerollButton.gameObject.SetActive(value: false);
			base.powerupMenuPanel.Hide();
			yield return new WaitForSecondsRealtime(1f);
			base.pauseController.UnPause();
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
