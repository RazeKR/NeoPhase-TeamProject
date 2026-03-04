using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace flanne.UI
{
	[RequireComponent(typeof(Toggle))]
	public class ToggleOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		private Toggle toggle;

		private void Start()
		{
			toggle = GetComponent<Toggle>();
		}

		public void OnSelect(BaseEventData eventData)
		{
			toggle.isOn = true;
		}
	}
}
