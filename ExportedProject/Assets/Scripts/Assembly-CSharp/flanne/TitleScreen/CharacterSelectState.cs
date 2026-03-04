using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace flanne.TitleScreen
{
	public class CharacterSelectState : TitleScreenState
	{
		public void OnClickRunes()
		{
			base.selectPanel.interactable = false;
			base.characterMenu.interactable = false;
			SaveSystem.data.checkedRunes = true;
			owner.ChangeState<RuneMenuCharacterState>();
		}

		private void OnClick(int i)
		{
			Loadout.CharacterSelection = base.characterMenu[i];
			Loadout.CharacterIndex = i;
			base.characterMenu.Hide();
			owner.ChangeState<GunSelectState>();
		}

		private void OnCancel(InputAction.CallbackContext obj)
		{
			OnCancel();
		}

		private void OnCancel()
		{
			base.selectPanel.Hide();
			base.characterMenu.Hide();
			owner.ChangeState<TitleMainMenuState>();
		}

		public override void Enter()
		{
			StartCoroutine(WaitToShowCR());
			base.checkRunesPromptArrow.enabled = SaveSystem.data.playedGame && !SaveSystem.data.checkedRunes;
		}

		public override void Exit()
		{
			base.runesButton.onClick.RemoveListener(OnClickRunes);
			base.characterMenu.onClick.RemoveListener(OnClick);
			base.loadoutBackButton.onClick.RemoveListener(OnCancel);
			base.input.FindAction("UI/Cancel").canceled -= OnCancel;
			Save();
		}

		private IEnumerator WaitToShowCR()
		{
			yield return new WaitForSeconds(0.2f);
			base.runesButton.onClick.AddListener(OnClickRunes);
			base.characterMenu.Show();
			base.characterMenu.onClick.AddListener(OnClick);
			base.loadoutBackButton.onClick.AddListener(OnCancel);
			base.input.FindAction("UI/Cancel").canceled += OnCancel;
		}
	}
}
