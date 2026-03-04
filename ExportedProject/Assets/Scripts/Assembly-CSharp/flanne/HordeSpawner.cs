using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class HordeSpawner : MonoBehaviour
	{
		[SerializeField]
		private Transform playerTransform;

		[SerializeField]
		private float spawnRadius;

		[NonSerialized]
		public float spawnRateMulitplier = 1f;

		[NonSerialized]
		public float healthMultiplier = 1f;

		[NonSerialized]
		public float eliteHealthMultiplier = 1f;

		[NonSerialized]
		public int enemyDamage = 1;

		[NonSerialized]
		public int eliteDamage = 1;

		[NonSerialized]
		public float speedMultiplier = 1f;

		[NonSerialized]
		public float eliteSpeedMultiplier = 1f;

		private List<SpawnSession> activeSpawners;

		private ObjectPooler OP;

		private void Awake()
		{
			activeSpawners = new List<SpawnSession>();
		}

		private void Update()
		{
			for (int i = 0; i < activeSpawners.Count; i++)
			{
				activeSpawners[i].timer -= Time.deltaTime;
				if (!(activeSpawners[i].timer < 0f))
				{
					continue;
				}
				if (CountActiveObjects(activeSpawners[i].monsterPrefab.name) < activeSpawners[i].maximum)
				{
					int num = Mathf.FloorToInt((float)activeSpawners[i].numPerSpawn * spawnRateMulitplier);
					for (int j = 0; j < num; j++)
					{
						Spawn(activeSpawners[i].monsterPrefab.name, activeSpawners[i].HP, activeSpawners[i].isElite);
					}
				}
				activeSpawners[i].timer += activeSpawners[i].spawnCooldown;
			}
		}

		public void LoadSpawners(List<SpawnSession> spawnSessions)
		{
			OP = ObjectPooler.SharedInstance;
			foreach (SpawnSession spawnSession in spawnSessions)
			{
				StartCoroutine(SpawnerLifeCycleCR(spawnSession));
			}
		}

		private void Spawn(string objectPoolTag, int HP, bool isElite)
		{
			GameObject pooledObject = OP.GetPooledObject(objectPoolTag);
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
			pooledObject.transform.position = playerTransform.position + new Vector3(vector.x, vector.y, 0f);
			Health component = pooledObject.GetComponent<Health>();
			if (component != null)
			{
				if (isElite)
				{
					component.maxHP = Mathf.FloorToInt((float)HP * eliteHealthMultiplier);
				}
				else
				{
					component.maxHP = Mathf.FloorToInt((float)HP * healthMultiplier);
				}
			}
			AIComponent component2 = pooledObject.GetComponent<AIComponent>();
			if (component2 != null)
			{
				if (isElite)
				{
					component2.damageToPlayer = eliteDamage;
					component2.maxMoveSpeed = component2.baseMaxMoveSpeed * eliteSpeedMultiplier;
					component2.acceleration = component2.baseAcceleration * eliteSpeedMultiplier;
				}
				else
				{
					component2.damageToPlayer = enemyDamage;
					component2.maxMoveSpeed = component2.baseMaxMoveSpeed * speedMultiplier;
					component2.acceleration = component2.baseAcceleration * speedMultiplier;
				}
			}
			pooledObject.SetActive(value: true);
		}

		private int CountActiveObjects(string objectPoolTag)
		{
			int num = 0;
			List<GameObject> allPooledObjects = OP.GetAllPooledObjects(objectPoolTag);
			for (int i = 0; i < allPooledObjects.Count; i++)
			{
				if (allPooledObjects[i].activeInHierarchy)
				{
					num++;
				}
			}
			return num;
		}

		private IEnumerator SpawnerLifeCycleCR(SpawnSession spawner)
		{
			if (spawner.isElite)
			{
				OP.AddObject(spawner.monsterPrefab.name, spawner.monsterPrefab, 1);
			}
			else
			{
				OP.AddObject(spawner.monsterPrefab.name, spawner.monsterPrefab, 1000);
			}
			yield return new WaitForSeconds(spawner.startTime);
			activeSpawners.Add(spawner);
			spawner.timer = 0f;
			yield return new WaitForSeconds(spawner.duration);
			activeSpawners.Remove(spawner);
		}
	}
}
