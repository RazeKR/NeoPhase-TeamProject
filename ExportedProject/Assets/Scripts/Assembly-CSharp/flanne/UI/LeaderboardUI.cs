using System;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class LeaderboardUI : MonoBehaviour
	{
		[SerializeField]
		private GameObject entryPrefab;

		[SerializeField]
		private LayoutGroup entriesLayout;

		[SerializeField]
		private TMP_Text retrievingDataTMP;

		[SerializeField]
		private int maxEntries;

		public async void SubmitAndShowsync(int score)
		{
			retrievingDataTMP.enabled = true;
			Leaderboard leaderboard = await SteamIntegration.Instance.GetLeaderboardAsync("Endless Mode");
			try
			{
				await leaderboard.SubmitScoreAsync(score);
				LeaderboardEntry[] array = await leaderboard.GetScoresFromFriendsAsync();
				if (array != null)
				{
					SetLeaderboards(array);
				}
				retrievingDataTMP.enabled = false;
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
		}

		private void SetLeaderboards(LeaderboardEntry[] lbEntries)
		{
			for (int i = 0; i < Mathf.Min(maxEntries, lbEntries.Length); i++)
			{
				GameObject obj = UnityEngine.Object.Instantiate(entryPrefab);
				obj.transform.SetParent(entriesLayout.transform);
				obj.transform.localScale = Vector3.one;
				LeaderboardUIEntry component = obj.GetComponent<LeaderboardUIEntry>();
				if (component != null)
				{
					component.rankTMP.text = lbEntries[i].GlobalRank.ToString();
					component.nameTMP.text = lbEntries[i].User.Name.Truncate(12);
					component.scoreTMP.text = lbEntries[i].Score.ToString();
				}
			}
		}
	}
}
