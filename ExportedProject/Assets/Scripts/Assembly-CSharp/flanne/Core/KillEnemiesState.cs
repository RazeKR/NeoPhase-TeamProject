using System.Collections;
using UnityEngine;

namespace flanne.Core
{
	public class KillEnemiesState : GameState
	{
		public override void Enter()
		{
			StartCoroutine(KillEnemiesCR());
			GameTimer.SharedInstance.Stop();
		}

		public override void Exit()
		{
		}

		private IEnumerator KillEnemiesCR()
		{
			base.screenFlash.Flash(1);
			LeanTween.scale(base.playerFogRevealer, new Vector3(5f, 5f, 1f), 0.4f).setEase(LeanTweenType.easeInCubic);
			yield return new WaitForSeconds(0.3f);
			GameObject[] array = GameObject.FindGameObjectsWithTag("Enemy");
			for (int i = 0; i < array.Length; i++)
			{
				array[i].GetComponent<Health>()?.AutoKill();
			}
			base.screenFlash.Flash(4);
			yield return new WaitForSeconds(1.5f);
			PauseController.SharedInstance.Pause();
			AudioManager.Instance.FadeOutMusic(0.5f);
			yield return new WaitForSecondsRealtime(0.5f);
			MapData mapData = SelectedMap.MapData;
			if (SaveSystem.data != null && !SaveSystem.data.characterUnlocks.unlocks[8] && mapData.name == "20M_Temple")
			{
				owner.ChangeState<HasturUnlockedState>();
			}
			else if (SaveSystem.data != null && !SaveSystem.data.characterUnlocks.unlocks[9] && mapData.name == "20M_PumpkinPatch")
			{
				owner.ChangeState<RavenUnlockedState>();
			}
			else
			{
				owner.ChangeState<PlayerSurvivedState>();
			}
		}
	}
}
