using System.Collections;
using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AISpawnPrefabNearPlayer", menuName = "AISpecials/AISpawnPrefabNearPlayer")]
	public class AISpawnPrefabNearPlayer : AISpecial
	{
		[SerializeField]
		private GameObject enemyPrefab;

		[SerializeField]
		private float spawnDistanceFromPlayer;

		[SerializeField]
		private int numToSpawn = 1;

		[SerializeField]
		private float delayBetweenSpawn;

		public override void Use(AIComponent ai, Transform target)
		{
			ai.StartCoroutine(SpawnCR(ai, target));
		}

		private IEnumerator SpawnCR(AIComponent ai, Transform target)
		{
			ObjectPooler OP = ObjectPooler.SharedInstance;
			OP.AddObject(enemyPrefab.name, enemyPrefab, 10);
			for (int i = 0; i < numToSpawn; i++)
			{
				GameObject pooledObject = OP.GetPooledObject(enemyPrefab.name);
				Vector3 normalized = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
				Vector3 position = target.position + normalized * spawnDistanceFromPlayer;
				pooledObject.transform.position = position;
				pooledObject.SetActive(value: true);
				yield return new WaitForSeconds(delayBetweenSpawn);
			}
		}
	}
}
