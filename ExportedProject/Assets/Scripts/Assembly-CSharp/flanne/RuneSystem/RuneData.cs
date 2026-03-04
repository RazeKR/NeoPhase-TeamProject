using System;
using UnityEngine;

namespace flanne.RuneSystem
{
	[CreateAssetMenu(fileName = "RuneData", menuName = "RuneData")]
	public class RuneData : ScriptableObject
	{
		public Sprite icon;

		public LocalizedString nameStringID;

		public LocalizedString descriptionStringID;

		public int costPerLevel;

		public Rune runePrefab;

		[NonSerialized]
		public int level;

		public string nameString => LocalizationSystem.GetLocalizedValue(nameStringID.key);

		public virtual string description => LocalizationSystem.GetLocalizedValue(descriptionStringID.key);

		public void Apply(PlayerController player)
		{
			UnityEngine.Object.Instantiate(runePrefab.gameObject).GetComponent<Rune>().Attach(player, level);
		}
	}
}
