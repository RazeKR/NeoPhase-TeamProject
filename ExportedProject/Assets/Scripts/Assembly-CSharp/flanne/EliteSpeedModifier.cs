using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "EliteSpeedModifier", menuName = "DifficultyMods/EliteSpeedModifier")]
	public class EliteSpeedModifier : DifficultyModifier
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float additionalSpeedPercent;

		public override void ModifyHordeSpawner(HordeSpawner hordeSpawner)
		{
			hordeSpawner.eliteSpeedMultiplier += additionalSpeedPercent;
		}
	}
}
