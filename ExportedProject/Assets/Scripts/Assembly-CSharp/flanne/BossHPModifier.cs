using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "BossHPModifier", menuName = "DifficultyMods/BossHPModifier")]
	public class BossHPModifier : DifficultyModifier
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float additionalHPPercent;

		public override void ModifyBossSpawner(BossSpawner bossSpawner)
		{
			bossSpawner.healthMultiplier += additionalHPPercent;
		}
	}
}
