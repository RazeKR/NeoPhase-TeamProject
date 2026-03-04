using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using flanne.Player;
using flanne.UI;
using flanne.UIExtensions;

namespace flanne.Core
{
	public class GameController : StateMachine
	{
		public PlayerInput playerInput;

		public PauseController pauseController;

		public PowerupGenerator powerupGenerator;

		public int numPowerupChoices = 5;

		public GameObject player;

		public PlayerHealth playerHealth;

		public PlayerXP playerXP;

		public GameObject playerFogRevealer;

		public CameraRig playerCameraRig;

		public Animator levelupAnimator;

		public ScreenFlash screenFlash;

		public ShootingCursor shootingCursor;

		[Header("Audio")]
		public AudioClip battleMusic;

		public SoundEffectSO youSurvivedSFX;

		public SoundEffectSO youDiedSFX;

		public SoundEffectSO powerupMenuSFX;

		public SoundEffectSO levelUpSFX;

		public SoundEffectSO gunEvoStartSFX;

		public SoundEffectSO gunEvoMenuSFX;

		public SoundEffectSO gunEvoEndSFX;

		[Header("UI")]
		public flanne.UIExtensions.Menu optionsMenu;

		public flanne.UI.Panel hud;

		public ChestUIController chestUIController;

		public flanne.UI.Panel powerupMenuPanel;

		public PowerupMenu powerupMenu;

		public Button powerupRerollButton;

		public flanne.UI.Panel devilDealMenuPanel;

		public PowerupMenu devilDealMenu;

		public Button devilDealLeaveButton;

		public EndScreenUIC endScreenUIC;

		public Button retryRunButton;

		public Button quitToTitleButton;

		public flanne.UI.Panel pauseMenu;

		public Button pauseResumeButton;

		public Button optionsButton;

		public Button synergiesButton;

		public Button giveupButton;

		public flanne.UI.Panel mouseAmmoUI;

		public flanne.UI.Panel powerupListUI;

		public flanne.UI.Panel loadoutUI;

		public flanne.UI.Panel gunEvoPanel;

		public flanne.UI.Menu gunEvoMenu;

		public flanne.UI.Panel haloUIPanel;

		public PowerupDescription haloUI;

		public Button takeHaloButton;

		public flanne.UI.Panel synergiesUIPanel;

		public Button synergiesUIBackButton;

		public ShootDetector shootDetector;

		public HitlessDetector hitlessDetector;

		public flanne.UI.Panel leaderBoardPanel;

		public LeaderboardUI leaderboardUI;

		public flanne.UI.Panel hasturUnlockedPanel;

		public Button hasturUnlockedConfirmButton;

		public flanne.UI.Panel ravenUnlockedPanel;

		public Button ravenUnlockedConfirmButton;

		private void Awake()
		{
			LeanTween.init(5000000);
			TagLayerUtil.Init();
		}

		private void Start()
		{
			ChangeState<InitState>();
		}
	}
}
