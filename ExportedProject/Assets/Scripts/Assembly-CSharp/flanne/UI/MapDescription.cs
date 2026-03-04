using TMPro;
using UnityEngine;

namespace flanne.UI
{
	public class MapDescription : DataUIBinding<MapData>
	{
		[SerializeField]
		private TMP_Text descriptionTMP;

		[SerializeField]
		private GameObject acensionModeUI;

		public override void Refresh()
		{
			descriptionTMP.text = base.data.description;
			acensionModeUI.SetActive(!base.data.darknessDisabled);
		}
	}
}
