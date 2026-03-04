using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class UIBar : MonoBehaviour
	{
		[SerializeField]
		private Slider slider;

		public void SetValue(int value)
		{
			slider.value = value;
		}

		public void SetMax(int maxValue)
		{
			slider.maxValue = maxValue;
		}

		public void SetValue(float value)
		{
			slider.value = value;
		}

		public void SetMax(float maxValue)
		{
			slider.maxValue = maxValue;
		}
	}
}
