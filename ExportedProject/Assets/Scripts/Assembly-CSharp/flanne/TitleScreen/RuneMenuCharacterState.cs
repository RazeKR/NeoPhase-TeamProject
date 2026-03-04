using UnityEngine.InputSystem;

namespace flanne.TitleScreen
{
	public class RuneMenuCharacterState : TitleScreenState
	{
		private void OnConfirmClick()
		{
			owner.ChangeState<CharacterSelectState>();
		}

		private void OnCancel(InputAction.CallbackContext obj)
		{
			owner.ChangeState<CharacterSelectState>();
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
