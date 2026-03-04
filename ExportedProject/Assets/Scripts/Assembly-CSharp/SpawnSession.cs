using System;
using UnityEngine;

[Serializable]
public class SpawnSession
{
	public GameObject monsterPrefab;

	public int HP = 1;

	public int maximum;

	public int numPerSpawn;

	public float spawnCooldown;

	public float startTime;

	public float duration;

	public bool isElite;

	[NonSerialized]
	public float timer;
}
