using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "EliteHPModifier", menuName = "DifficultyMods/EliteHPModifier")]
	public class EliteHPModifier : DifficultyModifier
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float additionalHPPercent;

		public override void ModifyHordeSpawner(HordeSpawner hordeSpawner)
		{
			hordeSpawner.eliteHealthMultiplier += additionalHPPercent;
		}
	}
}
