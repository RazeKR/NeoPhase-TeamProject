using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using flanne.Core;

namespace flanne.Player
{
	public class ReloadState : PlayerState
	{
		private IEnumerator reloadCoroutine;

		private void OnFireAction(InputAction.CallbackContext obj)
		{
			if (!base.ammo.outOfAmmo && !PauseController.isPaused)
			{
				owner.ChangeState<ShootingState>();
				if (reloadCoroutine != null)
				{
					StopCoroutine(reloadCoroutine);
					reloadCoroutine = null;
				}
				base.reloadBar.transform.parent.gameObject.SetActive(value: false);
			}
		}

		private void OnDisableToggleChange(object sender, bool isDisabled)
		{
			if (isDisabled)
			{
				owner.ChangeState<DisabledState>();
				if (reloadCoroutine != null)
				{
					StopCoroutine(reloadCoroutine);
					reloadCoroutine = null;
				}
				base.reloadBar.transform.parent.gameObject.SetActive(value: false);
			}
		}

		private void OnAmmoChanged(int currentAmmo)
		{
			if (currentAmmo > 0 && base.playerInput.actions["Fire"].ReadValue<float>() == 1f)
			{
				owner.ChangeState<ShootingState>();
			}
		}

		public override void Enter()
		{
			base.gun.StopShooting();
			base.playerInput.actions["Fire"].started += OnFireAction;
			owner.disableAction.ToggleEvent += OnDisableToggleChange;
			base.ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
			owner.moveSpeedMultiplier = 1f;
			owner.faceMouse = false;
			if (reloadCoroutine == null)
			{
				reloadCoroutine = ReloadCR();
				StartCoroutine(reloadCoroutine);
			}
			base.gun.SetAnimationTrigger("Reload");
			if (owner.gun.gunData.reloadSFXOverride == null)
			{
				owner.reloadStartSFX.Play();
			}
			else
			{
				owner.gun.gunData.reloadSFXOverride.Play();
			}
		}

		public override void Exit()
		{
			base.playerInput.actions["Fire"].started -= OnFireAction;
			owner.disableAction.ToggleEvent -= OnDisableToggleChange;
			base.ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
			base.gun.SetAnimationTrigger("Still");
		}

		private IEnumerator ReloadCR()
		{
			base.reloadBar.transform.parent.gameObject.SetActive(value: true);
			float timer = 0f;
			for (float reloadDuration = base.gun.reloadDuration; timer < reloadDuration; timer += Time.deltaTime)
			{
				base.reloadBar.value = timer / reloadDuration;
				yield return null;
			}
			base.ammo.Reload();
			owner.reloadEndSFX.Play();
			if (base.playerInput.actions["Fire"].ReadValue<float>() != 0f)
			{
				owner.ChangeState<ShootingState>();
			}
			else
			{
				owner.ChangeState<IdleState>();
			}
			base.reloadBar.transform.parent.gameObject.SetActive(value: false);
			reloadCoroutine = null;
		}
	}
}
