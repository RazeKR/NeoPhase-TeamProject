using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace flanne.UI
{
	public class ToolTipText : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public bool setToolTip;

		[SerializeField]
		private LocalizedString tooltipString;

		[NonSerialized]
		public string tooltip;

		private Tooltip TP;

		private void Start()
		{
			TP = Tooltip.Instance;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (setToolTip)
			{
				TP.ShowTooltip(LocalizationSystem.GetLocalizedValue(tooltipString.key));
			}
			else
			{
				TP.ShowTooltip(tooltip);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			TP.HideTooltip();
		}
	}
}
