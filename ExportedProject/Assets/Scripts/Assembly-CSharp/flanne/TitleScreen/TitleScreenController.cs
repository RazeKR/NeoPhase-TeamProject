using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using flanne.UI;
using flanne.UIExtensions;

namespace flanne.TitleScreen
{
	public class TitleScreenController : StateMachine
	{
		public InputActionAsset input;

		public flanne.UIExtensions.Panel leavesPanel;

		public flanne.UIExtensions.Panel logoPanel;

		public flanne.UIExtensions.Panel selectPanel;

		public flanne.UIExtensions.Panel runeMenuPanel;

		public flanne.UIExtensions.Panel modeSelectPanel;

		public flanne.UIExtensions.Panel mapSelectPanel;

		public flanne.UIExtensions.Panel emberpathPanel;

		public flanne.UIExtensions.Menu mainMenu;

		public flanne.UIExtensions.Menu languageMenu;

		public flanne.UIExtensions.Menu optionsMenu;

		public CharacterMenu characterMenu;

		public GunMenu gunMenu;

		public Button runesButton;

		public Button runeConfirmButton;

		public Animator eyes;

		public Image screenCover;

		public Image checkRunesPromptArrow;

		public GameModeMenu modeSelectMenu;

		public GameModeMenu mapSelectMenu;

		public AudioClip titleScreenMusic;

		public UnlockablesManager characterUnlocker;

		public UnlockablesManager gunUnlocker;

		public TieredUnlockManager runeUnlocker;

		public VictoryAchievedUI characterVictories;

		public VictoryAchievedUI gunVictories;

		public DifficultyController difficultyController;

		public UnlockAtDarkness templeUnlocker;

		public UnlockAtDarkness pumpkinPatchUnlocker;

		public RuneTreeUI swordRuneTree;

		public RuneTreeUI shieldRuneTree;

		public Button loadoutBackButton;

		public Button modeSelectStartButton;

		public Button modeSelectBackButton;

		public Button mapSelectStartButton;

		public Button mapSelectBackButton;

		private void Awake()
		{
			LeanTween.init(500000);
		}

		private void Start()
		{
			ChangeState<InitState>();
		}

		public void Save()
		{
			SaveSystem.data.points = PointsTracker.pts;
			SaveSystem.data.characterUnlocks = characterUnlocker.unlockData;
			SaveSystem.data.gunUnlocks = gunUnlocker.unlockData;
			SaveSystem.data.runeUnlocks = runeUnlocker.unlockData;
			SaveSystem.data.characterHighestClear = characterVictories.victories;
			SaveSystem.data.gunHighestClear = gunVictories.victories;
			SaveSystem.data.swordRuneSelections = swordRuneTree.GetSelections();
			SaveSystem.data.shieldRuneSelections = shieldRuneTree.GetSelections();
			SaveSystem.Save();
		}
	}
}
