using System.Collections;
using TMPro;
using UnityEngine;

namespace flanne.UI
{
	public class EndScreenUIC : MonoBehaviour
	{
		[SerializeField]
		private Panel youDiedPanel;

		[SerializeField]
		private Panel youSurvivedPanel;

		[SerializeField]
		private Panel screenCoverPanel;

		[SerializeField]
		private Panel[] scorePanels;

		[SerializeField]
		private Panel quitButtonPanel;

		[SerializeField]
		private TMP_Text timeSurvivedTMP;

		[SerializeField]
		private TMP_Text timeSurvivedScoreTMP;

		[SerializeField]
		private TMP_Text enemiesKilledTMP;

		[SerializeField]
		private TMP_Text enemiesKilledScoreTMP;

		[SerializeField]
		private TMP_Text levelsEarnedTMP;

		[SerializeField]
		private TMP_Text levelsEarnedScoreTMP;

		[SerializeField]
		private TMP_Text totalScoreTMP;

		[SerializeField]
		private LocalizedString timeSurvivedLabel;

		[SerializeField]
		private LocalizedString enemiesKilledLabel;

		[SerializeField]
		private LocalizedString levelsEarnedLabel;

		public void Show(bool survived)
		{
			if (survived)
			{
				youSurvivedPanel.Show();
			}
			else
			{
				youDiedPanel.Show();
			}
			StartCoroutine(ShowPanelsCR());
		}

		public void Hide()
		{
			youSurvivedPanel.Hide();
			youDiedPanel.Hide();
			Panel[] array = scorePanels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Hide();
			}
			quitButtonPanel.Hide();
		}

		public void SetScores(Score score)
		{
			timeSurvivedTMP.text = LocalizationSystem.GetLocalizedValue(timeSurvivedLabel.key) + " <color=#F5D6C1>(" + score.timeSurvivedString + ")</color>";
			timeSurvivedScoreTMP.text = score.timeSurvivedScore.ToString();
			enemiesKilledTMP.text = LocalizationSystem.GetLocalizedValue(enemiesKilledLabel.key) + " <color=#F5D6C1>(" + score.enemiesKilled + ")</color>";
			enemiesKilledScoreTMP.text = score.enemiesKilledScore.ToString();
			levelsEarnedTMP.text = LocalizationSystem.GetLocalizedValue(levelsEarnedLabel.key) + " <color=#F5D6C1>(" + score.levelsEarned + ")</color>";
			levelsEarnedScoreTMP.text = score.levelsEarnedScore.ToString();
			totalScoreTMP.text = score.totalScore.ToString();
		}

		private IEnumerator ShowPanelsCR()
		{
			yield return new WaitForSecondsRealtime(1.5f);
			Panel[] array = scorePanels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Show();
				yield return new WaitForSecondsRealtime(0.1f);
			}
			yield return new WaitForSecondsRealtime(1f);
			screenCoverPanel.Show();
			quitButtonPanel.Show();
		}
	}
}
