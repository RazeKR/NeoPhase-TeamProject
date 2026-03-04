using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class PowerupIcon : DataUIBinding<Powerup>
	{
		[SerializeField]
		private Image iconImage;

		[SerializeField]
		private ToolTipText tooltipText;

		public override void Refresh()
		{
			iconImage.sprite = base.data.icon;
			if (tooltipText != null)
			{
				tooltipText.tooltip = base.data.description;
			}
		}
	}
}
