using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using flanne.Pickups;

namespace flanne.Core
{
	public class CombatState : GameState
	{
		private void OnLevelUP(int level)
		{
			if (level == 20)
			{
				owner.ChangeState<GunEvoMenuState>();
				return;
			}
			base.levelUpSFX.Play();
			owner.ChangeState<PowerupMenuState>();
		}

		private void OnDeath()
		{
			owner.ChangeState<PlayerDeadState>();
		}

		private void OnChestPickup(object sender, object args)
		{
			owner.ChangeState<ChestState>();
		}

		private void OnDevilDealPickup(object sender, object args)
		{
			owner.ChangeState<DevilDealState>();
		}

		private void OnHaloPickup(object sender, object args)
		{
			owner.ChangeState<ShanaHaloState>();
		}

		private void OnTimerReached(object sender, object args)
		{
			owner.ChangeState<KillEnemiesState>();
		}

		private void OnPauseAction(InputAction.CallbackContext obj)
		{
			base.pauseController.Pause();
			owner.ChangeState<PauseState>();
		}

		public override void Enter()
		{
			base.playerXP.OnLevelChanged.AddListener(OnLevelUP);
			base.playerHealth.onDeath.AddListener(OnDeath);
			this.AddObserver(OnChestPickup, ChestPickup.ChestPickupEvent);
			this.AddObserver(OnDevilDealPickup, DevilDealPickup.DevilDealPickupEvent);
			this.AddObserver(OnHaloPickup, HaloPickup.HaloPickupEvent);
			this.AddObserver(OnTimerReached, GameTimer.TimeLimitNotification);
			base.playerInput.actions["Pause"].started += OnPauseAction;
			base.mouseAmmoUI.Show();
			base.shootingCursor.EnableGamepadCusor();
			EventSystem.current.SetSelectedGameObject(null);
		}

		public override void Exit()
		{
			base.playerXP.OnLevelChanged.RemoveListener(OnLevelUP);
			base.playerHealth.onDeath.RemoveListener(OnDeath);
			this.RemoveObserver(OnChestPickup, ChestPickup.ChestPickupEvent);
			this.RemoveObserver(OnDevilDealPickup, DevilDealPickup.DevilDealPickupEvent);
			this.RemoveObserver(OnHaloPickup, HaloPickup.HaloPickupEvent);
			this.RemoveObserver(OnTimerReached, GameTimer.TimeLimitNotification);
			base.playerInput.actions["Pause"].started -= OnPauseAction;
			base.mouseAmmoUI.Hide();
			base.shootingCursor.DisableGamepadCursor();
		}
	}
}
