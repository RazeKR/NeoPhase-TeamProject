using System.Collections;
using UnityEngine;

namespace flanne.UI
{
	[RequireComponent(typeof(Panel))]
	public class AutoShowPanel : MonoBehaviour
	{
		[SerializeField]
		private float startTime;

		[SerializeField]
		private float duration;

		private Panel panel;

		private void Start()
		{
			panel = GetComponent<Panel>();
			StartCoroutine(AutoShowCR());
		}

		private IEnumerator AutoShowCR()
		{
			yield return new WaitForSecondsRealtime(startTime);
			panel.Show();
			yield return new WaitForSecondsRealtime(duration);
			panel.Hide();
		}
	}
}
