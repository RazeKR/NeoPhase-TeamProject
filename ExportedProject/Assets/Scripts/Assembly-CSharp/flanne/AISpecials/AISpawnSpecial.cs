using System.Collections;
using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AISpawnSpecial", menuName = "AISpecials/AISpawnSpecial")]
	public class AISpawnSpecial : AISpecial
	{
		[SerializeField]
		private SoundEffectSO soundFX;

		[SerializeField]
		private float spawnWinduptime;

		[SerializeField]
		private int numSpawns;

		public override void Use(AIComponent ai, Transform target)
		{
			Spawner componentInChildren = ai.GetComponentInChildren<Spawner>();
			ai.StartCoroutine(WaitToSpawn(numSpawns, componentInChildren));
			ai.animator?.SetTrigger("Special");
		}

		private IEnumerator WaitToSpawn(int numSpawns, Spawner spawner)
		{
			for (int i = 0; i < numSpawns; i++)
			{
				yield return new WaitForSeconds(spawnWinduptime);
				soundFX?.Play();
				spawner?.Spawn();
			}
		}
	}
}
