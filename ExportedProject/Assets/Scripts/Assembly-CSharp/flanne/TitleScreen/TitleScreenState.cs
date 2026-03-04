using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using flanne.UI;
using flanne.UIExtensions;

namespace flanne.TitleScreen
{
	public abstract class TitleScreenState : State
	{
		protected TitleScreenController owner;

		protected InputActionAsset input => owner.input;

		protected flanne.UIExtensions.Panel leavesPanel => owner.leavesPanel;

		protected flanne.UIExtensions.Panel logoPanel => owner.logoPanel;

		protected flanne.UIExtensions.Panel selectPanel => owner.selectPanel;

		protected flanne.UIExtensions.Panel runeMenuPanel => owner.runeMenuPanel;

		protected flanne.UIExtensions.Panel modeSelectPanel => owner.modeSelectPanel;

		protected flanne.UIExtensions.Panel mapSelectPanel => owner.mapSelectPanel;

		protected flanne.UIExtensions.Panel emberpathPanel => owner.emberpathPanel;

		protected flanne.UIExtensions.Menu mainMenu => owner.mainMenu;

		protected flanne.UIExtensions.Menu languageMenu => owner.languageMenu;

		protected flanne.UIExtensions.Menu optionsMenu => owner.optionsMenu;

		protected CharacterMenu characterMenu => owner.characterMenu;

		protected GunMenu gunMenu => owner.gunMenu;

		protected Animator eyes => owner.eyes;

		protected Image screenCover => owner.screenCover;

		protected Image checkRunesPromptArrow => owner.checkRunesPromptArrow;

		protected GameModeMenu modeSelectMenu => owner.modeSelectMenu;

		protected GameModeMenu mapSelectMenu => owner.mapSelectMenu;

		protected AudioClip titleScreenMusic => owner.titleScreenMusic;

		protected UnlockablesManager characterUnlocker => owner.characterUnlocker;

		protected UnlockablesManager gunUnlocker => owner.gunUnlocker;

		protected TieredUnlockManager runeUnlocker => owner.runeUnlocker;

		protected VictoryAchievedUI characterVictories => owner.characterVictories;

		protected VictoryAchievedUI gunVictories => owner.gunVictories;

		protected DifficultyController difficultyController => owner.difficultyController;

		protected UnlockAtDarkness templeUnlocker => owner.templeUnlocker;

		protected UnlockAtDarkness pumpkinPatchUnlocker => owner.pumpkinPatchUnlocker;

		protected RuneTreeUI swordRuneTree => owner.swordRuneTree;

		protected RuneTreeUI shieldRuneTree => owner.shieldRuneTree;

		protected Button loadoutBackButton => owner.loadoutBackButton;

		protected Button runesButton => owner.runesButton;

		protected Button runeConfirmButton => owner.runeConfirmButton;

		protected Button modeSelectStartButton => owner.modeSelectStartButton;

		protected Button modeSelectBackButton => owner.modeSelectBackButton;

		protected Button mapSelectStartButton => owner.mapSelectStartButton;

		protected Button mapSelectBackButton => owner.mapSelectBackButton;

		private void Awake()
		{
			owner = GetComponentInParent<TitleScreenController>();
		}

		protected void Save()
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
