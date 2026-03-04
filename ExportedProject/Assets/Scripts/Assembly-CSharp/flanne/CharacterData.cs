using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData", order = 1)]
	public class CharacterData : ScriptableObject
	{
		public LocalizedString nameStringID;

		public LocalizedString descriptionStringID;

		public RuntimeAnimatorController animController;

		public RuntimeAnimatorController uiAnimController;

		public Sprite portrait;

		public Sprite icon;

		public int startHP;

		public GameObject passivePrefab;

		public PowerupPoolProfile exclusivePowerups;

		public string nameString => LocalizationSystem.GetLocalizedValue(nameStringID.key);

		public string description => LocalizationSystem.GetLocalizedValue(descriptionStringID.key);
	}
}
