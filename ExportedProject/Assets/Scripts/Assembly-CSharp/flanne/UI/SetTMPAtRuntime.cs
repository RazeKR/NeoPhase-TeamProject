using TMPro;
using UnityEngine;

namespace flanne.UI
{
	public class SetTMPAtRuntime : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text tmp;

		[SerializeField]
		private string text;

		private void Start()
		{
			tmp.text = text;
		}
	}
}
