using System;
using UnityEngine;

namespace flanne
{
	[Serializable]
	public class BossSpawn
	{
		public int maxHP;

		public GameObject bossPrefab;

		public float timeToSpawn;

		public Vector3 spawnPosition;

		public bool dontSpawnArena;

		public bool killAllOnSpawn;
	}
}
