using TMPro;
using UnityEngine;
using UnityEngine.UI;
using flanne.UIExtensions;

namespace flanne.UI
{
	public class CharacterDescription : MenuEntryDescription<CharacterMenu, CharacterData>
	{
		[SerializeField]
		private Image portrait;

		[SerializeField]
		private TMP_Text nameTMP;

		[SerializeField]
		private TMP_Text healthTMP;

		[SerializeField]
		private TMP_Text descriptionTMP;

		public override void SetProperties(CharacterData data)
		{
			portrait.sprite = data.portrait;
			nameTMP.text = data.nameString;
			healthTMP.text = data.startHP.ToString();
			descriptionTMP.text = data.description;
		}
	}
}
