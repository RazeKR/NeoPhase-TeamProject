using System.Collections;
using UnityEngine;

namespace flanne.Core
{
	public class InitState : GameState
	{
		private int fogRevealTweenID;

		public override void Enter()
		{
			PauseController.SharedInstance.Pause();
			StartCoroutine(WaitToShowStartBattle());
			AudioManager.Instance.SetLowPassFilter(isOn: true);
			AudioManager.Instance.PlayMusic(base.battleMusic);
			AudioManager.Instance.FadeInMusic(0.5f);
		}

		public override void Exit()
		{
			PauseController.SharedInstance.UnPause();
			AudioManager.Instance.SetLowPassFilter(isOn: false);
		}

		private IEnumerator WaitToShowStartBattle()
		{
			yield return new WaitForSecondsRealtime(0.5f);
			fogRevealTweenID = LeanTween.scale(base.playerFogRevealer, new Vector3(0.5f, 0.5f, 1f), 0.5f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(useUnScaledTime: true)
				.id;
			while (LeanTween.isTweening(fogRevealTweenID))
			{
				yield return null;
			}
			fogRevealTweenID = LeanTween.scale(base.playerFogRevealer, Vector3.one, 0.5f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(useUnScaledTime: true)
				.id;
			while (LeanTween.isTweening(fogRevealTweenID))
			{
				yield return null;
			}
			base.hud.Show();
			owner.ChangeState<CombatState>();
		}
	}
}
