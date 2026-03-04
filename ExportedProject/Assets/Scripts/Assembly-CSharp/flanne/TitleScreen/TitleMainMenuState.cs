using System.Collections;
using UnityEngine;

namespace flanne.TitleScreen
{
	public class TitleMainMenuState : TitleScreenState
	{
		public void OnClick(int i)
		{
			switch (i)
			{
			case 0:
				StartCoroutine(TransitionToLoadoutSelect());
				break;
			case 1:
				owner.ChangeState<LangaugeState>();
				break;
			case 2:
				owner.ChangeState<OptionsMenuState>();
				break;
			case 3:
				Application.Quit();
				break;
			}
		}

		public override void Enter()
		{
			Cursor.visible = true;
			StartCoroutine(WaitToShowMenuCR());
		}

		public override void Exit()
		{
			base.mainMenu.Hide();
			base.mainMenu.onClick.RemoveListener(OnClick);
		}

		private IEnumerator WaitToShowMenuCR()
		{
			base.eyes.SetTrigger("Open");
			yield return new WaitForSeconds(0.3f);
			base.logoPanel.Show();
			base.leavesPanel.Show();
			base.mainMenu.Show();
			base.mainMenu.onClick.AddListener(OnClick);
			base.emberpathPanel.Show();
		}

		private IEnumerator TransitionToLoadoutSelect()
		{
			base.logoPanel.Hide();
			base.leavesPanel.Hide();
			base.eyes.ResetTrigger("Open");
			base.eyes.SetTrigger("Close");
			base.mainMenu.Hide();
			base.emberpathPanel.Hide();
			yield return new WaitForSeconds(0.2f);
			base.selectPanel.Show();
			owner.ChangeState<CharacterSelectState>();
		}
	}
}
