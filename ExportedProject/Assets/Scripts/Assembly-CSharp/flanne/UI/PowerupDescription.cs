using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class PowerupDescription : DataUIBinding<Powerup>
	{
		[SerializeField]
		private Image icon;

		[SerializeField]
		private TMP_Text nameTMP;

		[SerializeField]
		private TMP_Text descriptionTMP;

		[SerializeField]
		private PowerupTreeUI powerupTreeUI;

		public override void Refresh()
		{
			if (icon != null)
			{
				icon.sprite = base.data.icon;
			}
			nameTMP.text = base.data.nameString;
			descriptionTMP.text = base.data.description;
			if (powerupTreeUI != null)
			{
				if (base.data.powerupTreeUIData != null)
				{
					powerupTreeUI.gameObject.SetActive(value: true);
					powerupTreeUI.data = base.data.powerupTreeUIData;
				}
				else
				{
					powerupTreeUI.gameObject.SetActive(value: false);
				}
			}
		}
	}
}
