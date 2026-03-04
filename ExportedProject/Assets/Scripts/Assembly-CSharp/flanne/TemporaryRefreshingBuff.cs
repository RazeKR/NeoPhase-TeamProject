using System.Collections;
using UnityEngine;

namespace flanne
{
	public class TemporaryRefreshingBuff : BuffPlayerStats
	{
		[SerializeField]
		private int duration;

		private IEnumerator buffCoroutine;

		private float _timer;

		public void Activate()
		{
			if (buffCoroutine == null)
			{
				buffCoroutine = BuffCR();
				StartCoroutine(buffCoroutine);
			}
			else
			{
				_timer = 0f;
			}
		}

		private IEnumerator BuffCR()
		{
			ApplyBuff();
			while (_timer < (float)duration)
			{
				_timer += Time.deltaTime;
				yield return null;
			}
			RemoveBuff();
			_timer = 0f;
			buffCoroutine = null;
		}
	}
}
