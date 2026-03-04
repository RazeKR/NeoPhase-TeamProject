using UnityEngine;
using UnityEngine.InputSystem;

namespace flanne.UI
{
	public class ControlSchemeUISwitcher : MonoBehaviour
	{
		[SerializeField]
		private PlayerInput playerInput;

		[SerializeField]
		private GameObject gamepadUI;

		[SerializeField]
		private GameObject kbmUI;

		private const string gamepadScheme = "Gamepad";

		private const string mouseScheme = "Keyboard&Mouse";

		private string previousControlScheme = "";

		private void Update()
		{
			if (previousControlScheme != playerInput.currentControlScheme)
			{
				if (playerInput.currentControlScheme == "Keyboard&Mouse")
				{
					kbmUI?.SetActive(value: true);
					gamepadUI?.SetActive(value: false);
				}
				else if (playerInput.currentControlScheme == "Gamepad")
				{
					gamepadUI?.SetActive(value: true);
					kbmUI?.SetActive(value: false);
				}
			}
		}
	}
}
