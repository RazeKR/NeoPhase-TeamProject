using UnityEngine;

namespace flanne.Core
{
	public class PlayerSurvivedState : GameState
	{
		private void OnClickRetry()
		{
			owner.ChangeState<TransitionToRetryState>();
		}

		private void OnClickQuit()
		{
			owner.ChangeState<TransitionToTitleState>();
		}

		public override void Enter()
		{
			base.retryRunButton.onClick.AddListener(OnClickRetry);
			base.quitToTitleButton.onClick.AddListener(OnClickQuit);
			AudioManager.Instance.SetLowPassFilter(isOn: true);
			base.youSurvivedSFX.Play();
			base.hud.Hide();
			Score score = ScoreCalculator.SharedInstance.GetScore();
			base.endScreenUIC.SetScores(score);
			base.endScreenUIC.Show(survived: true);
			base.powerupListUI.Show();
			base.loadoutUI.Show();
			if (!SelectedMap.MapData.achievementsDisabled)
			{
				CheckDifficultyUnlock();
				CheckAchievmentUnlocks();
			}
		}

		public override void Exit()
		{
			base.retryRunButton.onClick.RemoveListener(OnClickRetry);
			base.quitToTitleButton.onClick.RemoveListener(OnClickQuit);
			base.powerupListUI.Hide();
			base.loadoutUI.Hide();
			AudioManager.Instance.SetLowPassFilter(isOn: false);
		}

		private void CheckDifficultyUnlock()
		{
			if (Loadout.difficultyLevel == SaveSystem.data.difficultyUnlocked && Loadout.difficultyLevel < 15)
			{
				SaveSystem.data.difficultyUnlocked++;
			}
			int characterIndex = Loadout.CharacterIndex;
			if (SaveSystem.data.characterHighestClear[characterIndex] < Loadout.difficultyLevel)
			{
				SaveSystem.data.characterHighestClear[characterIndex] = Loadout.difficultyLevel;
			}
			int gunIndex = Loadout.GunIndex;
			if (SaveSystem.data.gunHighestClear[gunIndex] < Loadout.difficultyLevel)
			{
				SaveSystem.data.gunHighestClear[gunIndex] = Loadout.difficultyLevel;
			}
		}

		private void CheckAchievmentUnlocks()
		{
			SteamIntegration.UnlockAchievement("ACH_SURVIVE20");
			string text = Loadout.GunSelection.name;
			string text2 = Loadout.CharacterSelection.name;
			if (!base.shootDetector.usedShooting)
			{
				SteamIntegration.UnlockAchievement("ACH_PACIFIST");
			}
			if (!base.shootDetector.usedManualShooting && text2 == "Abby" && text == "GrenadeLauncherData")
			{
				SteamIntegration.UnlockAchievement("ACH_RECKLESS");
			}
			if (base.hitlessDetector.hitless)
			{
				SteamIntegration.UnlockAchievement("ACH_NIMBLE");
			}
			if (base.playerHealth.maxHP == 1)
			{
				SteamIntegration.UnlockAchievement("ACH_ON_THE_EDGE");
			}
			if (Object.FindObjectsOfType<Summon>().Length >= 8)
			{
				SteamIntegration.UnlockAchievement("ACH_CATCH_THEM_ALL");
			}
			if (Loadout.difficultyLevel >= 1)
			{
				SteamIntegration.UnlockAchievement("ACH_DARKNESS1");
			}
			if (Loadout.difficultyLevel >= 5)
			{
				SteamIntegration.UnlockAchievement("ACH_DARKNESS5");
			}
			if (Loadout.difficultyLevel >= 10)
			{
				SteamIntegration.UnlockAchievement("ACH_DARKNESS10");
			}
			if (Loadout.difficultyLevel >= 15)
			{
				SteamIntegration.UnlockAchievement("ACH_DARKNESS15");
				switch (text)
				{
				case "RevolverData":
					SteamIntegration.UnlockAchievement("ACH_REVOLVER");
					break;
				case "ShotgunData":
					SteamIntegration.UnlockAchievement("ACH_SHOTGUN");
					break;
				case "CrossbowData":
					SteamIntegration.UnlockAchievement("ACH_CROSSBOW");
					break;
				case "FlameCannon":
					SteamIntegration.UnlockAchievement("ACH_FLAME_CANNON");
					break;
				case "DualSMGsData":
					SteamIntegration.UnlockAchievement("ACH_SMGS");
					break;
				case "BatGunData":
					SteamIntegration.UnlockAchievement("ACH_BATGUN");
					break;
				case "GrenadeLauncherData":
					SteamIntegration.UnlockAchievement("ACH_GRENADE_LAUNCHER");
					break;
				case "MagicBowData":
					SteamIntegration.UnlockAchievement("ACH_MAGIC_BOW");
					break;
				case "SwordData":
					SteamIntegration.UnlockAchievement("ACH_CYCLONE_SWORD");
					break;
				case "SalvoKnivesData":
					SteamIntegration.UnlockAchievement("ACH_SALVO_KNIVES");
					break;
				case "SporeGunData":
					SteamIntegration.UnlockAchievement("ACH_WATERING_GUN");
					break;
				}
				switch (text2)
				{
				case "Shana":
					SteamIntegration.UnlockAchievement("ACH_SHANA");
					break;
				case "Diamond":
					SteamIntegration.UnlockAchievement("ACH_DIAMOND");
					break;
				case "Hina":
					SteamIntegration.UnlockAchievement("ACH_HINA");
					break;
				case "Scarlett":
					SteamIntegration.UnlockAchievement("ACH_SCARLETT");
					break;
				case "Spark":
					SteamIntegration.UnlockAchievement("ACH_SPARK");
					break;
				case "Lilith":
					SteamIntegration.UnlockAchievement("ACH_LILITH");
					break;
				case "Abby":
					SteamIntegration.UnlockAchievement("ACH_ABBY");
					break;
				case "Yuki":
					SteamIntegration.UnlockAchievement("ACH_YUKI");
					break;
				case "Luna":
					SteamIntegration.UnlockAchievement("ACH_LUNA");
					break;
				case "Hastur":
					SteamIntegration.UnlockAchievement("ACH_HASTUR");
					break;
				case "Raven":
					SteamIntegration.UnlockAchievement("ACH_RAVEN");
					break;
				case "Dasher":
					SteamIntegration.UnlockAchievement("ACH_DASHER");
					break;
				case "Katana":
					SteamIntegration.UnlockAchievement("ACH_KATANA");
					break;
				}
			}
		}
	}
}
