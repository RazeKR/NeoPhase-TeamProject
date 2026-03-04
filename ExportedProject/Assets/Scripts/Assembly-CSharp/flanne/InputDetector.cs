using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace flanne
{
	public class InputDetector : MonoBehaviour
	{
		public static InputDetector Instance;

		[SerializeField]
		private InputActionAsset inputs;

		public UnityEvent onControllerActive;

		public UnityEvent onControllerInactive;

		private InputAction _kbmAction;

		private InputAction _gamepadAction;

		public bool usingGamepad { get; private set; }

		private void OnKBMUsed(InputAction.CallbackContext context)
		{
			onControllerActive?.Invoke();
			usingGamepad = false;
			MonoBehaviour.print(usingGamepad);
		}

		private void OnGamepadUsed(InputAction.CallbackContext context)
		{
			onControllerInactive?.Invoke();
			usingGamepad = true;
			MonoBehaviour.print(usingGamepad);
		}

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			Object.DontDestroyOnLoad(base.gameObject);
		}

		private void Start()
		{
			_kbmAction = inputs.FindActionMap("InputDetector").FindAction("KBMUsed");
			_gamepadAction = inputs.FindActionMap("InputDetector").FindAction("GamepadUsed");
			_kbmAction.started += OnKBMUsed;
			_gamepadAction.started += OnGamepadUsed;
		}

		private void OnDestory()
		{
			_kbmAction.started -= OnKBMUsed;
			_gamepadAction.started -= OnGamepadUsed;
		}
	}
}
