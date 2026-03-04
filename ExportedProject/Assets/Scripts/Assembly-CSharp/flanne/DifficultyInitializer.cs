using UnityEngine;
using flanne.Core;

namespace flanne
{
	public class DifficultyInitializer : MonoBehaviour
	{
		[SerializeField]
		private HordeSpawner hordeSpawner;

		[SerializeField]
		private BossSpawner bossSpawner;

		[SerializeField]
		private GameController gameController;

		[SerializeField]
		private DifficultyModList modList;

		private void Start()
		{
			MapData mapData = SelectedMap.MapData;
			if (mapData != null && !mapData.darknessDisabled)
			{
				int difficultyLevel = Loadout.difficultyLevel;
				ApplyDifficulty(difficultyLevel);
			}
		}

		public void ApplyDifficulty(int diffLevel)
		{
			for (int i = 0; i < diffLevel + 1; i++)
			{
				modList.mods[i].ModifyHordeSpawner(hordeSpawner);
				modList.mods[i].ModifyBossSpawner(bossSpawner);
				modList.mods[i].ModifyGame(gameController);
			}
		}
	}
}
