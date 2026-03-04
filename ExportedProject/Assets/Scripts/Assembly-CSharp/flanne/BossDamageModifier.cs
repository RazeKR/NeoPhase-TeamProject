using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "BossDamageModifier", menuName = "DifficultyMods/BossDamageModifier")]
	public class BossDamageModifier : DifficultyModifier
	{
		[SerializeField]
		private int additionalDamage;

		public override void ModifyBossSpawner(BossSpawner bossSpawner)
		{
			bossSpawner.enemyDamage += additionalDamage;
		}
	}
}
