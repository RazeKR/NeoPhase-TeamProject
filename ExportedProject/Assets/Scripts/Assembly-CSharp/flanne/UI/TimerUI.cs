using TMPro;
using UnityEngine;

namespace flanne.UI
{
	public class TimerUI : MonoBehaviour
	{
		public TMP_Text TimerTMP;

		private GameTimer gameTimer;

		private void Start()
		{
			gameTimer = GameTimer.SharedInstance;
		}

		private void Update()
		{
			if (gameTimer.endless)
			{
				TimerTMP.text = gameTimer.TimeToString();
			}
			else
			{
				TimerTMP.text = gameTimer.TimeRemainingToString();
			}
		}
	}
}
