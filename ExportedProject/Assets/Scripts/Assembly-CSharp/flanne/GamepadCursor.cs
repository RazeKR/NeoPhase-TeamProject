using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace flanne
{
	public class GamepadCursor : MonoBehaviour
	{
		[SerializeField]
		private PlayerInput playerInput;

		[SerializeField]
		private RectTransform cursorTransform;

		[SerializeField]
		private RectTransform canvasTransform;

		[SerializeField]
		private Camera mainCamera;

		[SerializeField]
		private float cursorSpeed = 1000f;

		private const string gamepadScheme = "Gamepad";

		private const string mouseScheme = "Keyboard&Mouse";

		private string previousControlScheme = "";

		private bool _previousMouseState;

		private Mouse _virtualMouse;

		private Mouse _currentMouse;

		private void UpdateMotion()
		{
			if (_virtualMouse != null && Gamepad.current != null)
			{
				Vector2 vector = Gamepad.current.leftStick.ReadValue();
				vector *= cursorSpeed * Time.deltaTime;
				Vector2 vector2 = _virtualMouse.position.ReadValue() + vector;
				vector2.x = Mathf.Clamp(vector2.x, 0f, Screen.width);
				vector2.y = Mathf.Clamp(vector2.y, 0f, Screen.height);
				InputState.Change(_virtualMouse.position, vector2);
				InputState.Change(_virtualMouse.delta, vector);
				bool flag = Gamepad.current.aButton.IsPressed();
				if (_previousMouseState != Gamepad.current.aButton.isPressed)
				{
					_virtualMouse.CopyState<MouseState>(out var state);
					state.WithButton(MouseButton.Left, flag);
					InputState.Change(_virtualMouse, state);
					_previousMouseState = flag;
				}
				AnchorCursor(vector2);
			}
		}

		private void OnEnable()
		{
			if (_currentMouse == null)
			{
				_currentMouse = Mouse.current;
			}
			InitCursor();
			if (_virtualMouse == null)
			{
				_virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
			}
			else if (!_virtualMouse.added)
			{
				InputSystem.AddDevice(_virtualMouse);
			}
			InputUser.PerformPairingWithDevice(_virtualMouse, playerInput.user);
			Vector2 anchoredPosition = new Vector2(Screen.width / 2, Screen.height / 2);
			cursorTransform.anchoredPosition = anchoredPosition;
			Vector2 anchoredPosition2 = cursorTransform.anchoredPosition;
			InputState.Change(_virtualMouse.position, anchoredPosition2);
			InputSystem.onAfterUpdate += UpdateMotion;
		}

		private void OnDisable()
		{
			Cursor.visible = true;
			if (playerInput != null)
			{
				_ = playerInput.user;
				playerInput.user.UnpairDevice(_virtualMouse);
			}
			if (_virtualMouse != null && _virtualMouse.added)
			{
				InputSystem.RemoveDevice(_virtualMouse);
			}
			InputSystem.onAfterUpdate -= UpdateMotion;
		}

		private void Update()
		{
			if (previousControlScheme != playerInput.currentControlScheme)
			{
				OnControlsChanged(playerInput);
			}
		}

		private void AnchorCursor(Vector2 position)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, position, mainCamera, out var localPoint);
			cursorTransform.anchoredPosition = localPoint;
		}

		private void InitCursor()
		{
			if (playerInput.currentControlScheme == "Keyboard&Mouse")
			{
				cursorTransform.gameObject.SetActive(value: false);
				Cursor.visible = true;
			}
			else if (playerInput.currentControlScheme == "Gamepad")
			{
				cursorTransform.gameObject.SetActive(value: true);
				Cursor.visible = false;
			}
			previousControlScheme = playerInput.currentControlScheme;
		}

		private void OnControlsChanged(PlayerInput input)
		{
			if (input.currentControlScheme == "Keyboard&Mouse" && previousControlScheme != input.currentControlScheme)
			{
				cursorTransform.gameObject.SetActive(value: false);
				Cursor.visible = true;
				if (_currentMouse == null)
				{
					_currentMouse = Mouse.current;
				}
				_currentMouse.WarpCursorPosition(_virtualMouse.position.ReadValue());
				previousControlScheme = "Keyboard&Mouse";
			}
			else if (input.currentControlScheme == "Gamepad" && previousControlScheme != input.currentControlScheme)
			{
				cursorTransform.gameObject.SetActive(value: true);
				Cursor.visible = false;
				InputState.Change(_virtualMouse.position, _currentMouse.position.ReadValue());
				AnchorCursor(_currentMouse.position.ReadValue());
				previousControlScheme = "Gamepad";
			}
		}
	}
}
