using UnityEngine;
using flanne.Core;

namespace flanne
{
	[CreateAssetMenu(fileName = "BlankDifficultyModifier", menuName = "DifficultyMods/BlankDifficultyModifier")]
	public class DifficultyModifier : ScriptableObject
	{
		public LocalizedString descriptionStringID;

		public string description => LocalizationSystem.GetLocalizedValue(descriptionStringID.key);

		public virtual void ModifyHordeSpawner(HordeSpawner hordeSpawner)
		{
		}

		public virtual void ModifyBossSpawner(BossSpawner bossSpawner)
		{
		}

		public virtual void ModifyGame(GameController gameController)
		{
		}
	}
}
