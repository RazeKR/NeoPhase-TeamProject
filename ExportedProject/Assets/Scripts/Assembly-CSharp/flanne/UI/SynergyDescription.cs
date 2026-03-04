using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class SynergyDescription : DataUIBinding<Powerup>
	{
		[SerializeField]
		private Image icon;

		[SerializeField]
		private TMP_Text nameTMP;

		[SerializeField]
		private TMP_Text descriptionTMP;

		[SerializeField]
		private TMP_Text prereqsTMP;

		public override void Refresh()
		{
			if (icon != null)
			{
				icon.sprite = base.data.icon;
			}
			nameTMP.text = base.data.nameString;
			descriptionTMP.text = base.data.description;
			string text = "";
			for (int i = 0; i < base.data.prereqs.Count; i++)
			{
				text += base.data.prereqs[i].nameString;
				if (i + 1 < base.data.prereqs.Count)
				{
					text += " + ";
				}
			}
			prereqsTMP.text = text;
		}
	}
}
