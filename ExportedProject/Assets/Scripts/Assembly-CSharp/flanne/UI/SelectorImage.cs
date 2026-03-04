using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace flanne.UI
{
	public class SelectorImage : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		[SerializeField]
		private Image img;

		public void OnSelect(BaseEventData eventData)
		{
			img.enabled = true;
		}

		public void OnDeselect(BaseEventData eventData)
		{
			img.enabled = false;
		}
	}
}
