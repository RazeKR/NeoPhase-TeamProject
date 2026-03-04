using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace flanne.UI
{
	public class PointerDetector : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		public UnityEvent onEnter;

		public UnityEvent onExit;

		public UnityEvent onSelect;

		public UnityEvent onDeselect;

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			onEnter.Invoke();
		}

		public void OnPointerExit(PointerEventData pointerEventData)
		{
			onExit.Invoke();
		}

		public void OnSelect(BaseEventData eventData)
		{
			onSelect.Invoke();
		}

		public void OnDeselect(BaseEventData eventData)
		{
			onDeselect.Invoke();
		}
	}
}
