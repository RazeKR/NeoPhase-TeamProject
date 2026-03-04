using TMPro;
using UnityEngine;

namespace flanne.UI
{
	public class MapEntry : DataUIBinding<MapData>
	{
		[SerializeField]
		private TMP_Text labelTMP;

		public override void Refresh()
		{
			if (labelTMP != null)
			{
				labelTMP.text = base.data.nameString;
			}
		}
	}
}
