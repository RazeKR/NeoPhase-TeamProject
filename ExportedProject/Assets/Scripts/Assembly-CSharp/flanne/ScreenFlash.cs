using System.Collections;
using UnityEngine;
using flanne.UI;

namespace flanne
{
	public class ScreenFlash : MonoBehaviour
	{
		[SerializeField]
		private Panel flashPanel;

		private IEnumerator flashhCoroutine;

		public void Flash(int numTimes)
		{
			if (flashhCoroutine != null)
			{
				StopCoroutine(flashhCoroutine);
				flashhCoroutine = null;
			}
			else
			{
				flashhCoroutine = FlashCR(numTimes);
				StartCoroutine(flashhCoroutine);
			}
		}

		private IEnumerator FlashCR(int numTimes)
		{
			for (int i = 0; i < numTimes; i++)
			{
				flashPanel.Show();
				yield return new WaitForSecondsRealtime(0.05f);
				flashPanel.Hide();
				yield return new WaitForSecondsRealtime(0.05f);
			}
			flashhCoroutine = null;
		}
	}
}
