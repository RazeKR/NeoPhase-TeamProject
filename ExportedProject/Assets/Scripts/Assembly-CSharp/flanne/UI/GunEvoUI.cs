using TMPro;
using UnityEngine;
using UnityEngine.UI;
using flanne.UIExtensions;

namespace flanne.UI
{
	public class GunEvoUI : DataUI<GunEvolution>
	{
		[SerializeField]
		private Image icon;

		[SerializeField]
		private TMP_Text nameTMP;

		[SerializeField]
		private TMP_Text descriptionTMP;

		protected override void SetProperties()
		{
			icon.sprite = base.data.icon;
			nameTMP.text = base.data.nameString;
			descriptionTMP.text = base.data.description;
		}
	}
}
