using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using flanne.Player;
using flanne.UI;
using flanne.UIExtensions;

namespace flanne.Core
{
	public abstract class GameState : State
	{
		protected GameController owner;

		protected PlayerInput playerInput => owner.playerInput;

		protected PauseController pauseController => owner.pauseController;

		protected PowerupGenerator powerupGenerator => owner.powerupGenerator;

		protected int numPowerupChoices => owner.numPowerupChoices;

		protected GameObject player => owner.player;

		protected PlayerHealth playerHealth => owner.playerHealth;

		protected PlayerXP playerXP => owner.playerXP;

		protected GameObject playerFogRevealer => owner.playerFogRevealer;

		protected CameraRig playerCameraRig => owner.playerCameraRig;

		protected AudioClip battleMusic => owner.battleMusic;

		protected SoundEffectSO youSurvivedSFX => owner.youSurvivedSFX;

		protected SoundEffectSO youDiedSFX => owner.youDiedSFX;

		protected SoundEffectSO powerupMenuSFX => owner.powerupMenuSFX;

		protected SoundEffectSO levelUpSFX => owner.levelUpSFX;

		protected SoundEffectSO gunEvoStartSFX => owner.gunEvoStartSFX;

		protected SoundEffectSO gunEvoMenuSFX => owner.gunEvoMenuSFX;

		protected SoundEffectSO gunEvoEndSFX => owner.gunEvoEndSFX;

		protected Animator levelupAnimator => owner.levelupAnimator;

		protected ScreenFlash screenFlash => owner.screenFlash;

		protected ShootingCursor shootingCursor => owner.shootingCursor;

		protected flanne.UIExtensions.Menu optionsMenu => owner.optionsMenu;

		protected flanne.UI.Panel hud => owner.hud;

		protected flanne.UI.Panel powerupMenuPanel => owner.powerupMenuPanel;

		protected PowerupMenu powerupMenu => owner.powerupMenu;

		protected Button powerupRerollButton => owner.powerupRerollButton;

		protected flanne.UI.Panel devilDealMenuPanel => owner.devilDealMenuPanel;

		protected PowerupMenu devilDealMenu => owner.devilDealMenu;

		protected Button devilDealLeaveButton => owner.devilDealLeaveButton;

		protected ChestUIController chestUIController => owner.chestUIController;

		protected EndScreenUIC endScreenUIC => owner.endScreenUIC;

		protected Button retryRunButton => owner.retryRunButton;

		protected Button quitToTitleButton => owner.quitToTitleButton;

		protected flanne.UI.Panel pauseMenu => owner.pauseMenu;

		protected Button pauseResumeButton => owner.pauseResumeButton;

		protected Button optionsButton => owner.optionsButton;

		protected Button synergiesButton => owner.synergiesButton;

		protected Button giveupButton => owner.giveupButton;

		protected flanne.UI.Panel mouseAmmoUI => owner.mouseAmmoUI;

		protected flanne.UI.Panel powerupListUI => owner.powerupListUI;

		protected flanne.UI.Panel loadoutUI => owner.loadoutUI;

		protected flanne.UI.Panel gunEvoPanel => owner.gunEvoPanel;

		protected flanne.UI.Menu gunEvoMenu => owner.gunEvoMenu;

		protected flanne.UI.Panel haloUIPanel => owner.haloUIPanel;

		protected PowerupDescription haloUI => owner.haloUI;

		protected Button takeHaloButton => owner.takeHaloButton;

		protected flanne.UI.Panel synergiesUIPanel => owner.synergiesUIPanel;

		protected Button synergiesUIBackButton => owner.synergiesUIBackButton;

		protected ShootDetector shootDetector => owner.shootDetector;

		protected HitlessDetector hitlessDetector => owner.hitlessDetector;

		protected flanne.UI.Panel leaderBoardPanel => owner.leaderBoardPanel;

		protected LeaderboardUI leaderboardUI => owner.leaderboardUI;

		protected flanne.UI.Panel hasturUnlockedPanel => owner.hasturUnlockedPanel;

		protected Button hasturUnlockedConfirmButton => owner.hasturUnlockedConfirmButton;

		protected flanne.UI.Panel ravenUnlockedPanel => owner.ravenUnlockedPanel;

		protected Button ravenUnlockedConfirmButton => owner.ravenUnlockedConfirmButton;

		private void Awake()
		{
			owner = GetComponentInParent<GameController>();
		}
	}
}
