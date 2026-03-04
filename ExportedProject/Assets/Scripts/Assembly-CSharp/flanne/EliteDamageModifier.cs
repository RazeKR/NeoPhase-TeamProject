using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "EliteDamageModifier", menuName = "DifficultyMods/EliteDamageModifier")]
	public class EliteDamageModifier : DifficultyModifier
	{
		[SerializeField]
		private int additionalDamage;

		public override void ModifyHordeSpawner(HordeSpawner hordeSpawner)
		{
			hordeSpawner.eliteDamage += additionalDamage;
		}
	}
}
