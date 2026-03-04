using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace flanne
{
	public class SteamIntegration : MonoBehaviour
	{
		public static SteamIntegration Instance;

		private void Awake()
		{
			if (Instance == null)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				Instance = this;
				try
				{
					SteamClient.Init(1966900u);
					return;
				}
				catch (Exception message)
				{
					Debug.Log(message);
					return;
				}
			}
			if (Instance != null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private void Update()
		{
			SteamClient.RunCallbacks();
		}

		private void OnApplicationQuit()
		{
			SteamClient.Shutdown();
		}

		public static void UnlockAchievement(string id)
		{
			new Achievement(id).Trigger();
		}

		public static void ClearAchievement(string id)
		{
			new Achievement(id).Clear();
		}

		public async Task<Leaderboard> GetLeaderboardAsync(string leaderboardName)
		{
			try
			{
				return (await SteamUserStats.FindOrCreateLeaderboardAsync(leaderboardName, LeaderboardSort.Descending, LeaderboardDisplay.Numeric)).Value;
			}
			catch (Exception message)
			{
				Debug.Log(message);
				return default(Leaderboard);
			}
		}

		private void ClearAll()
		{
			ClearAchievement("ACH_SURVIVE20");
			ClearAchievement("ACH_DARKNESS1");
			ClearAchievement("ACH_DARKNESS5");
			ClearAchievement("ACH_DARKNESS10");
			ClearAchievement("ACH_DARKNESS15");
			ClearAchievement("ACH_RECKLESS");
		}

		private async void SubmitScore(int score)
		{
			try
			{
				SteamClient.Init(1966900u);
			}
			catch (Exception)
			{
			}
			LeaderboardUpdate? leaderboardUpdate = await (await GetLeaderboardAsync("Endless Mode")).SubmitScoreAsync(score);
			Debug.Log("Submitted: " + leaderboardUpdate.Value.Changed.ToString() + ", Score: " + leaderboardUpdate.Value.Score);
		}

		private async void ReplaceScore(int score)
		{
			try
			{
				SteamClient.Init(1966900u);
			}
			catch (Exception)
			{
			}
			LeaderboardUpdate? leaderboardUpdate = await (await GetLeaderboardAsync("Endless Mode")).ReplaceScore(score);
			Debug.Log("Submitted: " + leaderboardUpdate.Value.Changed.ToString() + ", Score: " + leaderboardUpdate.Value.Score);
		}
	}
}
