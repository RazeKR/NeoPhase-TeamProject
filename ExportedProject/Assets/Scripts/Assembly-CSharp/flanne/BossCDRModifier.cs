using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "BossCDRModifier", menuName = "DifficultyMods/BossCDRModifier")]
	public class BossCDRModifier : DifficultyModifier
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float cooldownReduction;

		public override void ModifyBossSpawner(BossSpawner bossSpawner)
		{
			bossSpawner.cooldownRate += cooldownReduction;
		}
	}
}
