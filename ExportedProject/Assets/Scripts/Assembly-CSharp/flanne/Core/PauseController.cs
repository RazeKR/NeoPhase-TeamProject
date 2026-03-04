using System.Collections;
using UnityEngine;

namespace flanne.Core
{
	public class PauseController : MonoBehaviour
	{
		public static PauseController SharedInstance;

		private IEnumerator pauseCoroutine;

		private static BoolToggle _isPaused = new BoolToggle(b: false);

		private float _timeScale = 1f;

		public static bool isPaused => _isPaused.value;

		private void Awake()
		{
			SharedInstance = this;
		}

		private void Start()
		{
			pauseCoroutine = null;
		}

		public void Pause()
		{
			_isPaused.Flip();
			Time.timeScale = 0f;
		}

		public void Pause(float duration)
		{
			if (pauseCoroutine != null)
			{
				_isPaused.UnFlip();
				StopCoroutine(pauseCoroutine);
			}
			pauseCoroutine = PauseCR(duration);
			StartCoroutine(pauseCoroutine);
		}

		public void UnPause()
		{
			_isPaused.UnFlip();
			if (!_isPaused.value)
			{
				Time.timeScale = _timeScale;
			}
		}

		public void SetTimeScale(float timeScale)
		{
			_timeScale = timeScale;
			if (!isPaused)
			{
				Time.timeScale = _timeScale;
			}
		}

		private IEnumerator PauseCR(float duration)
		{
			Pause();
			yield return new WaitForSecondsRealtime(duration);
			UnPause();
			pauseCoroutine = null;
		}
	}
}
