using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using flanne.UI;

namespace flanne.Player
{
	public class ShootingState : PlayerState
	{
		private IEnumerator _changeStateCoroutine;

		private void OnAmmoChanged(int amount)
		{
			if (amount == 0 && _changeStateCoroutine == null)
			{
				_changeStateCoroutine = WaitToChangeState<ReloadState>();
				StartCoroutine(_changeStateCoroutine);
			}
		}

		private void OnReloadAction(InputAction.CallbackContext obj)
		{
			if (!base.gun.gunData.disableManualReload)
			{
				owner.ChangeState<ReloadState>();
			}
		}

		private void OnCancelFire(InputAction.CallbackContext obj)
		{
			if (base.ammo.outOfAmmo || (OptionsSetter.AutoReloadEnabled && !base.gun.gunData.disableManualReload))
			{
				_changeStateCoroutine = WaitToChangeState<ReloadState>();
			}
			else
			{
				_changeStateCoroutine = WaitToChangeState<IdleState>();
			}
			StartCoroutine(_changeStateCoroutine);
		}

		private void OnDisableToggleChange(object sender, bool isDisabled)
		{
			if (isDisabled)
			{
				if (_changeStateCoroutine != null)
				{
					StopCoroutine(_changeStateCoroutine);
					_changeStateCoroutine = null;
				}
				owner.ChangeState<DisabledState>();
			}
		}

		public override void Enter()
		{
			base.playerInput.actions["Fire"].canceled += OnCancelFire;
			base.playerInput.actions["Reload"].started += OnReloadAction;
			base.playerInput.actions["Aim"].canceled += OnCancelFire;
			base.ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
			owner.disableAction.ToggleEvent += OnDisableToggleChange;
			base.gun.StartShooting();
			owner.moveSpeedMultiplier = Mathf.Min(1f, base.stats[StatType.WalkSpeed].Modify(0.35f));
			owner.faceMouse = true;
		}

		public override void Exit()
		{
			base.playerInput.actions["Fire"].canceled -= OnCancelFire;
			base.playerInput.actions["Reload"].started -= OnReloadAction;
			base.playerInput.actions["Aim"].canceled -= OnCancelFire;
			base.ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
			owner.disableAction.ToggleEvent -= OnDisableToggleChange;
			base.gun.StopShooting();
		}

		private IEnumerator WaitToChangeState<T>() where T : PlayerState
		{
			base.playerInput.actions["Fire"].canceled -= OnCancelFire;
			base.playerInput.actions["Reload"].started -= OnReloadAction;
			base.ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
			while (!base.gun.shotReady)
			{
				yield return null;
			}
			owner.ChangeState<T>();
			_changeStateCoroutine = null;
		}
	}
}
