namespace flanne.Player
{
	public class DisabledState : PlayerState
	{
		private void OnDisableToggleChange(object sender, bool isDisabled)
		{
			if (!isDisabled)
			{
				if (base.ammo.outOfAmmo)
				{
					owner.ChangeState<ReloadState>();
				}
				else if (base.playerInput.actions["Fire"].ReadValue<float>() != 0f)
				{
					owner.ChangeState<ShootingState>();
				}
				else
				{
					owner.ChangeState<IdleState>();
				}
			}
		}

		public override void Enter()
		{
			base.gun.StopShooting();
			owner.disableAction.ToggleEvent += OnDisableToggleChange;
			owner.moveSpeedMultiplier = 1f;
			owner.faceMouse = false;
		}

		public override void Exit()
		{
			owner.disableAction.ToggleEvent -= OnDisableToggleChange;
		}
	}
}
