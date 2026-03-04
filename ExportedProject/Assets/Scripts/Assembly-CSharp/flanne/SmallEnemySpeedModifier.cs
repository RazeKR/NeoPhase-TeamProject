using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "SmallEnemySpeedModifier", menuName = "DifficultyMods/SmallEnemySpeedModifier")]
	public class SmallEnemySpeedModifier : DifficultyModifier
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float additionalSpeedPercent;

		public override void ModifyHordeSpawner(HordeSpawner hordeSpawner)
		{
			hordeSpawner.speedMultiplier += additionalSpeedPercent;
		}
	}
}
