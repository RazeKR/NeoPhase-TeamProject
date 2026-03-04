using System.Collections;
using UnityEngine;

namespace flanne.Core
{
	public class PlayerDeadState : GameState
	{
		private void OnClickRetry()
		{
			owner.ChangeState<TransitionToRetryState>();
		}

		private void OnClickQuit()
		{
			owner.ChangeState<TransitionToTitleState>();
		}

		public override void Enter()
		{
			GameTimer.SharedInstance.Stop();
			AIController.SharedInstance.playerRepel = true;
			base.playerCameraRig.enabled = false;
			StartCoroutine(LoseVisionCR());
			base.retryRunButton.onClick.AddListener(OnClickRetry);
			base.quitToTitleButton.onClick.AddListener(OnClickQuit);
			AudioManager.Instance.SetLowPassFilter(isOn: true);
		}

		public override void Exit()
		{
			base.retryRunButton.onClick.RemoveListener(OnClickRetry);
			base.quitToTitleButton.onClick.RemoveListener(OnClickQuit);
			base.powerupListUI.Hide();
			base.loadoutUI.Hide();
			AudioManager.Instance.SetLowPassFilter(isOn: false);
		}

		private IEnumerator LoseVisionCR()
		{
			LeanTween.scale(base.playerFogRevealer, new Vector3(0.7f, 0.7f, 1f), 1f).setEase(LeanTweenType.easeOutBack);
			yield return new WaitForSeconds(1.5f);
			LeanTween.scale(base.playerFogRevealer, new Vector3(0f, 0f, 1f), 1f).setEase(LeanTweenType.easeInCubic);
			AudioManager.Instance.FadeOutMusic(0.5f);
			yield return new WaitForSeconds(0.5f);
			base.hud.Hide();
			base.youDiedSFX.Play();
			Score score = ScoreCalculator.SharedInstance.GetScore();
			base.endScreenUIC.SetScores(score);
			base.endScreenUIC.Show(survived: false);
			base.powerupListUI.Show();
			base.loadoutUI.Show();
			PauseController.SharedInstance.Pause();
			if (SelectedMap.MapData != null && SelectedMap.MapData.endless)
			{
				base.leaderBoardPanel.Show();
				base.leaderboardUI.SubmitAndShowsync(score.totalScore);
			}
		}
	}
}
