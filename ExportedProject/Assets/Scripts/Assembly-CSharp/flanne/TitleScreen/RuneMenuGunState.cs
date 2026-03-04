using UnityEngine.InputSystem;

namespace flanne.TitleScreen
{
	public class RuneMenuGunState : TitleScreenState
	{
		private void OnConfirmClick()
		{
			owner.ChangeState<GunSelectState>();
		}

		private void OnCancel(InputAction.CallbackContext obj)
		{
			owner.ChangeState<GunSelectState>();
		}

		public override void Enter()
		{
			base.runeMenuPanel.Show();
			base.runeConfirmButton.onClick.AddListener(OnConfirmClick);
			base.input.FindAction("UI/Cancel").canceled += OnCancel;
			base.checkRunesPromptArrow.enabled = false;
		}

		public override void Exit()
		{
			base.runeMenuPanel.Hide();
			base.runeConfirmButton.onClick.RemoveListener(OnConfirmClick);
			base.selectPanel.interactable = true;
			base.input.FindAction("UI/Cancel").canceled -= OnCancel;
			Save();
		}
	}
}
