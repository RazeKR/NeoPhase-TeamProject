using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace flanne.Core
{
	public class TransitionToRetryState : GameState
	{
		public override void Enter()
		{
			StartCoroutine(WaitToReload());
			Save();
		}

		private void Save()
		{
			PointsTracker.pts += ScoreCalculator.SharedInstance.GetScore().totalScore;
			if (SaveSystem.data != null)
			{
				SaveSystem.data.points = PointsTracker.pts;
				SaveSystem.Save();
			}
		}

		private IEnumerator WaitToReload()
		{
			base.endScreenUIC.Hide();
			yield return new WaitForSecondsRealtime(1.5f);
			PauseController.SharedInstance.UnPause();
			SceneManager.LoadScene("Battle", LoadSceneMode.Single);
		}
	}
}
