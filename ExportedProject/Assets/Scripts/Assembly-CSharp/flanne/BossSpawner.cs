using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class BossSpawner : MonoBehaviour
	{
		[SerializeField]
		private Transform playerTransform;

		[SerializeField]
		private GameObject arenaMonsterPrefab;

		[NonSerialized]
		public float healthMultiplier = 1f;

		[NonSerialized]
		public int enemyDamage = 1;

		[NonSerialized]
		public float cooldownRate = 1f;

		private ObjectPooler OP;

		public void LoadSpawners(List<BossSpawn> spawners)
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(arenaMonsterPrefab.name, arenaMonsterPrefab, 1);
			foreach (BossSpawn spawner in spawners)
			{
				OP.AddObject(spawner.bossPrefab.name, spawner.bossPrefab, 1);
				StartCoroutine(WaitToSpawnCR(spawner));
			}
		}

		private IEnumerator WaitToSpawnCR(BossSpawn spawner)
		{
			yield return new WaitForSeconds(spawner.timeToSpawn);
			if (spawner.killAllOnSpawn)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Enemy");
				for (int i = 0; i < array.Length; i++)
				{
					array[i].GetComponent<Health>()?.AutoKill();
				}
			}
			GameObject pooledObject = OP.GetPooledObject(spawner.bossPrefab.name);
			pooledObject.transform.position = playerTransform.position + spawner.spawnPosition;
			Health component = pooledObject.GetComponent<Health>();
			if (component != null)
			{
				component.maxHP = Mathf.FloorToInt((float)spawner.maxHP * healthMultiplier);
			}
			AIComponent component2 = pooledObject.GetComponent<AIComponent>();
			if (component2 != null)
			{
				component2.damageToPlayer = enemyDamage;
				component2.specialCooldown /= cooldownRate;
			}
			pooledObject.SetActive(value: true);
			if (!spawner.dontSpawnArena)
			{
				GameObject obj = UnityEngine.Object.Instantiate(arenaMonsterPrefab);
				obj.transform.position = playerTransform.position;
				obj.SetActive(value: true);
			}
		}
	}
}
