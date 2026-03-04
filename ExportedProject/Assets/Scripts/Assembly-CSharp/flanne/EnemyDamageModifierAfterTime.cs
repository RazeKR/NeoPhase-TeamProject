using System.Collections;
using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "EnemyDamageModifierAfterTime", menuName = "DifficultyMods/EnemyDamageModifierAfterTime")]
	public class EnemyDamageModifierAfterTime : DifficultyModifier
	{
		[SerializeField]
		private int additionalDamage;

		[SerializeField]
		private float timeRemainingToActivate;

		public override void ModifyHordeSpawner(HordeSpawner hordeSpawner)
		{
			hordeSpawner.StartCoroutine(WaitToIncreaseDamage(hordeSpawner));
		}

		private IEnumerator WaitToIncreaseDamage(HordeSpawner hordeSpawner)
		{
			yield return null;
			float timeLimit = GameTimer.SharedInstance.timeLimit;
			yield return new WaitForSeconds(timeLimit - timeRemainingToActivate);
			hordeSpawner.enemyDamage += additionalDamage;
		}
	}
}
