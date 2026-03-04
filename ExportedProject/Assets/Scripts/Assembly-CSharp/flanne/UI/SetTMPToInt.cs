using TMPro;
using UnityEngine;

namespace flanne.UI
{
	public class SetTMPToInt : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text tmp;

		[SerializeField]
		private int minDigits;

		public void SetToInt(int value)
		{
			string text = "";
			for (int i = 0; i < minDigits; i++)
			{
				text += "0";
			}
			tmp.text = value.ToString(text);
		}
	}
}
