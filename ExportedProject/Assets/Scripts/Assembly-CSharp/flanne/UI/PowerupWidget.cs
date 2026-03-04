using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class PowerupWidget : Widget<PowerupProperties>
	{
		[SerializeField]
		private Image icon;

		[SerializeField]
		private TMP_Text nameTMP;

		[SerializeField]
		private TMP_Text descriptionTMP;

		public override void SetProperties(PowerupProperties properties)
		{
			Powerup powerup = properties.powerup;
			if (icon != null)
			{
				icon.sprite = powerup.icon;
			}
			if (nameTMP != null)
			{
				nameTMP.text = powerup.nameString;
			}
			if (descriptionTMP != null)
			{
				descriptionTMP.text = powerup.description;
			}
		}
	}
}
