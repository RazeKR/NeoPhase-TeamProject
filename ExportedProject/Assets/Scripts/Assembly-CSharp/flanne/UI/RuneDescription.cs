using TMPro;
using UnityEngine;
using UnityEngine.UI;
using flanne.RuneSystem;

namespace flanne.UI
{
	public class RuneDescription : DataUIBinding<RuneData>
	{
		[SerializeField]
		private Image iconImage;

		[SerializeField]
		private TMP_Text costTMP;

		[SerializeField]
		private TMP_Text nameTMP;

		[SerializeField]
		private TMP_Text descriptionTMP;

		public override void Refresh()
		{
			iconImage.sprite = base.data.icon;
			costTMP.text = base.data.costPerLevel.ToString();
			nameTMP.text = base.data.nameString;
			descriptionTMP.text = base.data.description;
		}
	}
}
