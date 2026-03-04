using System;
using TMPro;
using UnityEngine;

namespace flanne
{
	public class VictoryAchievedUI : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] uiObjs;

		[SerializeField]
		private Color normalTextColor;

		[SerializeField]
		private Color maxVictoryTextColor;

		[SerializeField]
		private int maxVictoryPossible;

		private TMP_Text[] achievedUIs;

		public int[] victories { get; private set; }

		private void Start()
		{
			achievedUIs = new TMP_Text[uiObjs.Length];
			for (int i = 0; i < uiObjs.Length; i++)
			{
				achievedUIs[i] = uiObjs[i].GetComponentInChildren<TMP_Text>();
			}
		}

		public void SetProperties(int[] victoryAchieved)
		{
			if (victoryAchieved == null)
			{
				victoryAchieved = new int[achievedUIs.Length];
				for (int i = 0; i < achievedUIs.Length; i++)
				{
					victoryAchieved[i] = -1;
				}
			}
			if (victoryAchieved.Length < achievedUIs.Length)
			{
				int num = victoryAchieved.Length;
				Array.Resize(ref victoryAchieved, achievedUIs.Length);
				for (int j = num; j < achievedUIs.Length; j++)
				{
					victoryAchieved[j] = -1;
				}
			}
			for (int k = 0; k < achievedUIs.Length; k++)
			{
				uiObjs[k].SetActive(victoryAchieved[k] != -1);
				achievedUIs[k].text = victoryAchieved[k].ToString();
				if (victoryAchieved[k] == maxVictoryPossible)
				{
					achievedUIs[k].color = maxVictoryTextColor;
				}
				else
				{
					achievedUIs[k].color = normalTextColor;
				}
			}
			victories = victoryAchieved;
		}
	}
}
