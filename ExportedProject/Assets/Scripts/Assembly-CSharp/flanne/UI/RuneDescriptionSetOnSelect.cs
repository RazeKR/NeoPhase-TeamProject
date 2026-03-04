using UnityEngine;
using UnityEngine.EventSystems;

namespace flanne.UI
{
	public class RuneDescriptionSetOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		[SerializeField]
		private RuneIcon runeIcon;

		[SerializeField]
		private RuneDescription runeDescription;

		public void OnSelect(BaseEventData eventData)
		{
			runeDescription.data = runeIcon.data;
		}
	}
}
