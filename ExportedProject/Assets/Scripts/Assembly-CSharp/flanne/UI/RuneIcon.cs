using TMPro;
using UnityEngine;
using UnityEngine.UI;
using flanne.RuneSystem;

namespace flanne.UI
{
	public class RuneIcon : DataUIBinding<RuneData>
	{
		[SerializeField]
		private Image iconImage;

		[SerializeField]
		private TMP_Text levelTMP;

		public override void Refresh()
		{
			iconImage.sprite = base.data.icon;
			levelTMP.text = base.data.level.ToString();
		}
	}
}
