using System;
using UnityEngine;

namespace flanne
{
	public class GameTimer : MonoBehaviour
	{
		public static GameTimer SharedInstance;

		public static string TimeLimitNotification = "GameTimer.TimeLimitNotification";

		public static string OneSecondLeftNotification = "GameTimer.OneSecondLeftNotification";

		[NonSerialized]
		public float timeLimit = -1f;

		[NonSerialized]
		public bool endless;

		private bool oneSecondWarningSent;

		private bool _isPlaying = true;

		public float timer { get; private set; }

		private void Awake()
		{
			SharedInstance = this;
		}

		private void Update()
		{
			if (_isPlaying)
			{
				timer += Time.deltaTime;
				if (timer >= timeLimit - 1f && !oneSecondWarningSent)
				{
					oneSecondWarningSent = true;
					this.PostNotification(OneSecondLeftNotification, null);
					timer = timeLimit - 1f;
				}
				if (timer >= timeLimit && !endless)
				{
					this.PostNotification(TimeLimitNotification, null);
					timer = timeLimit;
				}
			}
		}

		public void Start()
		{
			_isPlaying = true;
		}

		public void Stop()
		{
			_isPlaying = false;
		}

		public string TimeToString()
		{
			int num = Mathf.FloorToInt(timer / 60f);
			int num2 = Mathf.FloorToInt(timer % 60f);
			return num.ToString("00") + ":" + num2.ToString("00");
		}

		public string TimeRemainingToString()
		{
			float num = timeLimit - timer;
			int num2 = Mathf.FloorToInt(num / 60f);
			int num3 = Mathf.FloorToInt(num % 60f);
			return num2.ToString("00") + ":" + num3.ToString("00");
		}
	}
}
