using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "SmallEnemyHPModifier", menuName = "DifficultyMods/SmallEnemyHPModifier")]
	public class SmallEnemyHPModifier : DifficultyModifier
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float additionalHPPercent;

		public override void ModifyHordeSpawner(HordeSpawner hordeSpawner)
		{
			hordeSpawner.healthMultiplier += additionalHPPercent;
		}
	}
}
