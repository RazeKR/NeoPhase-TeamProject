using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace flanne.UI
{
	[RequireComponent(typeof(Selectable))]
	public class SelectOnHover : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
	{
		private Selectable selectable;

		public void OnPointerEnter(PointerEventData eventData)
		{
			selectable.Select();
		}

		private void Start()
		{
			selectable = GetComponent<Selectable>();
		}
	}
}
