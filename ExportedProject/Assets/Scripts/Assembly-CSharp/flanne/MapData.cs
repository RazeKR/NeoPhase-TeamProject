using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "MapData", menuName = "MapData")]
	public class MapData : ScriptableObject
	{
		public LocalizedString nameStringID;

		public LocalizedString descriptionStringID;

		public GameObject mapPrefab;

		public bool achievementsDisabled;

		public bool darknessDisabled;

		public bool runesDisabled;

		public int numPowerupsRepeat = 1;

		public float timeLimit;

		public List<SpawnSession> spawnSessions;

		public List<BossSpawn> bossSpawns;

		public bool endless;

		public float endlessLoopTime;

		public List<SpawnSession> endlessSpawnSessions;

		public List<BossSpawn> endlessBossSpawn;

		public string nameString => LocalizationSystem.GetLocalizedValue(nameStringID.key);

		public string description => LocalizationSystem.GetLocalizedValue(descriptionStringID.key);
	}
}
