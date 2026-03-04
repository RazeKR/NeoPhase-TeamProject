using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using flanne.RuneSystem;

namespace flanne.TitleScreen
{
	public class GunSelectState : TitleScreenState
	{
		public void OnClickRunes()
		{
			base.selectPanel.interactable = false;
			base.gunMenu.interactable = false;
			SaveSystem.data.checkedRunes = true;
			owner.ChangeState<RuneMenuGunState>();
		}

		private void OnClick(int i)
		{
			owner.ChangeState<ModeSelectState>();
			Loadout.GunSelection = base.gunMenu[i];
			Loadout.GunIndex = i;
			List<RuneData> activeRunes = base.swordRuneTree.GetActiveRunes();
			activeRunes.AddRange(base.shieldRuneTree.GetActiveRunes());
			Loadout.RuneSelection = activeRunes;
			base.selectPanel.interactable = false;
			base.gunMenu.interactable = false;
		}

		private void OnCancel(InputAction.CallbackContext obj)
		{
			OnCancel();
		}

		private void OnCancel()
		{
			owner.ChangeState<CharacterSelectState>();
			base.gunMenu.Hide();
		}

		public override void Enter()
		{
			base.selectPanel.interactable = true;
			base.gunMenu.interactable = true;
			StartCoroutine(WaitToShowCR());
		}

		public override void Exit()
		{
			base.runesButton.onClick.RemoveListener(OnClickRunes);
			base.gunMenu.onClick.RemoveListener(OnClick);
			base.loadoutBackButton.onClick.RemoveListener(OnCancel);
			base.input.FindAction("UI/Cancel").canceled -= OnCancel;
			Save();
		}

		private IEnumerator WaitToShowCR()
		{
			yield return new WaitForSeconds(0.2f);
			base.runesButton.onClick.AddListener(OnClickRunes);
			base.gunMenu.Show();
			base.gunMenu.onClick.AddListener(OnClick);
			base.loadoutBackButton.onClick.AddListener(OnCancel);
			base.input.FindAction("UI/Cancel").canceled += OnCancel;
		}
	}
}
