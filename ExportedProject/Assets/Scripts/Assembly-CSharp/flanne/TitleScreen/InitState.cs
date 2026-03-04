using System.Collections;
using UnityEngine;

namespace flanne.TitleScreen
{
	public class InitState : TitleScreenState
	{
		public override void Enter()
		{
			StartCoroutine(WaitToLoadCR());
			AudioManager.Instance.PlayMusic(base.titleScreenMusic);
			AudioManager.Instance.FadeInMusic(5f);
			SaveSystem.Load();
			base.characterUnlocker.LoadData(SaveSystem.data.characterUnlocks);
			base.gunUnlocker.LoadData(SaveSystem.data.gunUnlocks);
			base.runeUnlocker.LoadData(SaveSystem.data.runeUnlocks);
			base.characterVictories.SetProperties(SaveSystem.data.characterHighestClear);
			base.gunVictories.SetProperties(SaveSystem.data.gunHighestClear);
			PointsTracker.pts = SaveSystem.data.points;
			base.swordRuneTree.SetSelections(SaveSystem.data.swordRuneSelections);
			base.shieldRuneTree.SetSelections(SaveSystem.data.shieldRuneSelections);
			base.difficultyController.Init(Mathf.Clamp(SaveSystem.data.difficultyUnlocked, 0, 15));
			base.templeUnlocker.CheckUnlock(Mathf.Clamp(SaveSystem.data.difficultyUnlocked, 0, 15));
			base.pumpkinPatchUnlocker.CheckUnlock(Mathf.Clamp(SaveSystem.data.difficultyUnlocked, 0, 15));
		}

		private IEnumerator WaitToLoadCR()
		{
			yield return new WaitForSeconds(0.5f);
			base.screenCover.enabled = false;
			owner.ChangeState<TitleMainMenuState>();
		}
	}
}
