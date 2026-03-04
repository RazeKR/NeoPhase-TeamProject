using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace flanne.UI
{
	public class DoubleClickDetector : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		public UnityEvent onDoubleClick;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.clickCount == 2)
			{
				onDoubleClick?.Invoke();
			}
		}
	}
}
