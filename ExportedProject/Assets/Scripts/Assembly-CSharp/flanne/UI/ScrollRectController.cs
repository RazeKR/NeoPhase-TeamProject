using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class ScrollRectController : MonoBehaviour
	{
		[SerializeField]
		private Scrollbar scrollBar;

		[Range(0f, 1f)]
		[SerializeField]
		private float scrollbarIncrements;

		public void IncrementScrollbar()
		{
			scrollBar.value += scrollbarIncrements;
		}

		public void DecrementScrollbar()
		{
			scrollBar.value -= scrollbarIncrements;
		}
	}
}
