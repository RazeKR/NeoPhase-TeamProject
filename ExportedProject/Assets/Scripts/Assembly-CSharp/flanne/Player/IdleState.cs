using System.Collections;
using UnityEngine.InputSystem;
using flanne.Core;

namespace flanne.Player
{
	public class IdleState : PlayerState
	{
		private void OnFireAction(InputAction.CallbackContext obj)
		{
			if (!PauseController.isPaused)
			{
				if (base.ammo.outOfAmmo)
				{
					owner.ChangeState<ReloadState>();
				}
				else
				{
					owner.ChangeState<ShootingState>();
				}
			}
		}

		private void OnReloadAction(InputAction.CallbackContext obj)
		{
			if (!PauseController.isPaused && !base.ammo.fullOnAmmo && !base.gun.gunData.disableManualReload)
			{
				owner.ChangeState<ReloadState>();
			}
		}

		private void OnAmmoChange(int amountChanged)
		{
			if (base.ammo.outOfAmmo)
			{
				owner.ChangeState<ReloadState>();
			}
		}

		private void OnDisableToggleChange(object sender, bool isDisabled)
		{
			if (isDisabled)
			{
				owner.ChangeState<DisabledState>();
			}
		}

		public override void Enter()
		{
			base.gun.StopShooting();
			base.playerInput.actions["Fire"].performed += OnFireAction;
			base.playerInput.actions["Reload"].started += OnReloadAction;
			base.ammo.OnAmmoChanged.AddListener(OnAmmoChange);
			owner.disableAction.ToggleEvent += OnDisableToggleChange;
			owner.moveSpeedMultiplier = 1f;
			owner.faceMouse = false;
			if (base.ammo.outOfAmmo && base.gun.gunData != null)
			{
				StartCoroutine(WaitToExitToReloadStateCR());
			}
		}

		public override void Exit()
		{
			base.playerInput.actions["Fire"].performed -= OnFireAction;
			base.playerInput.actions["Reload"].started -= OnReloadAction;
			base.ammo.OnAmmoChanged.RemoveListener(OnAmmoChange);
			owner.disableAction.ToggleEvent -= OnDisableToggleChange;
		}

		private IEnumerator WaitToExitToReloadStateCR()
		{
			yield return null;
			owner.ChangeState<ReloadState>();
		}
	}
}
