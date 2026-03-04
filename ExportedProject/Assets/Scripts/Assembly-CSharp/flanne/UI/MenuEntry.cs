using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace flanne.UI
{
	public class MenuEntry : Button, ICancelHandler, IEventSystemHandler
	{
		public UnityEvent onSelect;

		public UnityEvent onCancel;

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			Select();
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (onSelect != null)
			{
				onSelect.Invoke();
			}
		}

		public void OnCancel(BaseEventData eventData)
		{
			if (onCancel != null)
			{
				onCancel.Invoke();
			}
		}
	}
}
