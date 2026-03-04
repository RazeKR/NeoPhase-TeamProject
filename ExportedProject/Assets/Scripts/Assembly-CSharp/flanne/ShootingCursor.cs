using System;
using UnityEngine;
using UnityEngine.InputSystem;
using flanne.Core;

namespace flanne
{
	public class ShootingCursor : MonoBehaviour
	{
		public static ShootingCursor Instance;

		[NonSerialized]
		public bool autoAim;

		[SerializeField]
		private PlayerInput playerInput;

		[SerializeField]
		private InputActionAsset inputs;

		[SerializeField]
		private RectTransform cursorTransform;

		[SerializeField]
		private Canvas canvas;

		[SerializeField]
		private float fixedDistance = 100f;

		private const string gamepadScheme = "Gamepad";

		private const string mouseScheme = "Keyboard&Mouse";

		private string previousControlScheme = "";

		private InputAction _pointAction;

		private InputAction _aimAction;

		private InputAction _autoToggleAction;

		private Vector2 _lastGamepadVector = Vector2.zero;

		private bool _usingGamepad;

		private bool _disableGamepadCursor;

		public Vector2 cursorPosition { get; private set; }

		public bool usingGamepad => _usingGamepad;

		private void OnPoint(InputAction.CallbackContext context)
		{
			if (!autoAim || PauseController.isPaused)
			{
				cursorPosition = context.ReadValue<Vector2>();
				AnchorCursor(cursorPosition);
			}
		}

		private void OnAim(InputAction.CallbackContext context)
		{
			_lastGamepadVector = context.ReadValue<Vector2>();
		}

		private void OnAimCancel(InputAction.CallbackContext context)
		{
			_lastGamepadVector = Vector2.zero;
		}

		private void OnAutoAimToggle(InputAction.CallbackContext context)
		{
			if (context.ReadValue<float>() == 1f)
			{
				autoAim = !autoAim;
			}
		}

		private void OnControlsChanged(PlayerInput input)
		{
			if (input.currentControlScheme == "Keyboard&Mouse" && previousControlScheme != input.currentControlScheme)
			{
				_usingGamepad = false;
			}
			else if (input.currentControlScheme == "Gamepad" && previousControlScheme != input.currentControlScheme)
			{
				_usingGamepad = true;
			}
		}

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private void OnEnable()
		{
			Cursor.visible = false;
			_pointAction = inputs.FindActionMap("PlayerMap").FindAction("Point");
			_aimAction = inputs.FindActionMap("PlayerMap").FindAction("Aim");
			_autoToggleAction = inputs.FindActionMap("PlayerMap").FindAction("AutoAim");
			_pointAction.performed += OnPoint;
			_aimAction.performed += OnAim;
			_aimAction.canceled += OnAimCancel;
			_autoToggleAction.performed += OnAutoAimToggle;
		}

		private void OnDisable()
		{
			_pointAction.performed -= OnPoint;
			_aimAction.performed -= OnAim;
			_aimAction.canceled -= OnAimCancel;
			_autoToggleAction.performed -= OnAutoAimToggle;
		}

		private void Update()
		{
			if (previousControlScheme != playerInput.currentControlScheme)
			{
				OnControlsChanged(playerInput);
			}
			if (autoAim && !PauseController.isPaused)
			{
				AutoAim();
			}
			else if (_usingGamepad && !_disableGamepadCursor)
			{
				Vector2 vector = new Vector2(Screen.width / 2, Screen.height / 2);
				if (_lastGamepadVector != Vector2.zero)
				{
					cursorPosition = vector + _lastGamepadVector.normalized * fixedDistance;
					AnchorCursor(cursorPosition);
				}
			}
		}

		public void EnableGamepadCusor()
		{
			_disableGamepadCursor = false;
		}

		public void DisableGamepadCursor()
		{
			_disableGamepadCursor = true;
		}

		private void AnchorCursor(Vector2 position)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, position, canvas.worldCamera, out var localPoint);
			cursorTransform.position = canvas.transform.TransformPoint(localPoint);
		}

		private void AutoAim()
		{
			Vector2 vector = new Vector2(Screen.width / 2, Screen.height / 2);
			Vector2 center = Camera.main.ScreenToWorldPoint(vector);
			cursorPosition = Camera.main.WorldToScreenPoint(AIController.GetClosestAIPos(center));
			AnchorCursor(cursorPosition);
		}
	}
}
