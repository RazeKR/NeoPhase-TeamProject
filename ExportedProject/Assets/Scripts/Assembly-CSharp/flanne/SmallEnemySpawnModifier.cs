using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "SmallEnemySpawnModifier", menuName = "DifficultyMods/SmallEnemySpawnModifier")]
	public class SmallEnemySpawnModifier : DifficultyModifier
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float additionalSpawnRatePercent;

		public override void ModifyHordeSpawner(HordeSpawner hordeSpawner)
		{
			hordeSpawner.spawnRateMulitplier += additionalSpawnRatePercent;
		}
	}
}
