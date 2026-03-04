using System.Collections;
using UnityEngine;

namespace flanne
{
	public class MapInitializer : MonoBehaviour
	{
		[SerializeField]
		private HordeSpawner hordeSpawner;

		[SerializeField]
		private BossSpawner bossSpawner;

		[SerializeField]
		private MapData defaultMap;

		[SerializeField]
		private GameTimer gameTimer;

		private void Start()
		{
			MapData mapData = SelectedMap.MapData;
			if (mapData == null)
			{
				mapData = defaultMap;
			}
			if (mapData.endless)
			{
				StartCoroutine(EndlessLoop(mapData));
				gameTimer.endless = true;
			}
			else
			{
				hordeSpawner.LoadSpawners(mapData.spawnSessions);
				bossSpawner.LoadSpawners(mapData.bossSpawns);
				gameTimer.timeLimit = mapData.timeLimit;
			}
			PowerupGenerator.Instance.InitPowerupPool(mapData.numPowerupsRepeat);
			Object.Instantiate(mapData.mapPrefab);
		}

		private IEnumerator EndlessLoop(MapData mapData)
		{
			hordeSpawner.LoadSpawners(mapData.spawnSessions);
			bossSpawner.LoadSpawners(mapData.bossSpawns);
			yield return new WaitForSeconds(mapData.timeLimit);
			int cycle = 1;
			while (true)
			{
				hordeSpawner.healthMultiplier = 1f + Mathf.Pow(cycle, 3f);
				hordeSpawner.eliteHealthMultiplier = 1f + Mathf.Pow(cycle, 2f);
				hordeSpawner.speedMultiplier += 0.2f;
				bossSpawner.healthMultiplier += 3f;
				bossSpawner.cooldownRate += 0.25f;
				hordeSpawner.LoadSpawners(mapData.endlessSpawnSessions);
				bossSpawner.LoadSpawners(mapData.endlessBossSpawn);
				cycle++;
				yield return new WaitForSeconds(mapData.endlessLoopTime);
			}
		}
	}
}
