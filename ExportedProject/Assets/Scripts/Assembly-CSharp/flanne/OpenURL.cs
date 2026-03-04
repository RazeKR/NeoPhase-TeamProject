using UnityEngine;

namespace flanne
{
	public class OpenURL : MonoBehaviour
	{
		[SerializeField]
		private string url;

		public void GoToURL()
		{
			Application.OpenURL(url);
		}
	}
}
