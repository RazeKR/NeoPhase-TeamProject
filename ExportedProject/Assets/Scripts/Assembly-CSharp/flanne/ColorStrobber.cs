using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace flanne
{
	public class ColorStrobber : MonoBehaviour
	{
		[SerializeField]
		private Graphic targetGraphic;

		[SerializeField]
		private Color[] strobeColors;

		[SerializeField]
		private float timeBetweenColors;

		private void OnEnable()
		{
			StartCoroutine(StartStrobeCR());
		}

		private IEnumerator StartStrobeCR()
		{
			int index = 0;
			while (true)
			{
				targetGraphic.color = strobeColors[index];
				index++;
				if (index >= strobeColors.Length)
				{
					index = 0;
				}
				yield return new WaitForSecondsRealtime(timeBetweenColors);
			}
		}
	}
}
