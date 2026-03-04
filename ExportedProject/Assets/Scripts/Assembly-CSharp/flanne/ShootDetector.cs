using UnityEngine;
using UnityEngine.InputSystem;
using flanne.Core;

namespace flanne
{
	public class ShootDetector : MonoBehaviour
	{
		[SerializeField]
		private Gun playerGun;

		[SerializeField]
		private InputActionAsset inputs;

		private InputAction _shootAction;

		public bool usedShooting { get; private set; }

		public bool usedManualShooting { get; private set; }

		private void OnShoot()
		{
			usedShooting = true;
		}

		private void OnManualShoot(InputAction.CallbackContext context)
		{
			if (!PauseController.isPaused)
			{
				usedManualShooting = true;
			}
		}

		private void Start()
		{
			usedShooting = false;
			usedManualShooting = false;
			playerGun.OnShoot.AddListener(OnShoot);
			_shootAction = inputs.FindActionMap("PlayerMap").FindAction("Fire");
			_shootAction.performed += OnManualShoot;
		}

		private void OnDestroy()
		{
			playerGun.OnShoot.RemoveListener(OnShoot);
			_shootAction.performed -= OnManualShoot;
		}
	}
}
