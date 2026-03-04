using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public static class EnemyFinder
	{
		public static GameObject GetRandomEnemy(Vector2 center, Vector2 range)
		{
			List<GameObject> list = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
			list.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("EnemyChampion")));
			GameObject gameObject = null;
			while (list.Count > 0 && gameObject == null)
			{
				GameObject gameObject2 = list[Random.Range(0, list.Count)];
				if (Mathf.Abs(gameObject2.transform.position.x - center.x) < range.x && Mathf.Abs(gameObject2.transform.position.y - center.y) < range.y)
				{
					gameObject = gameObject2;
				}
				else
				{
					list.Remove(gameObject2);
				}
			}
			return gameObject;
		}

		public static GameObject GetClosestEnemy(Vector2 center)
		{
			return GetClosestEnemy(center, null);
		}

		public static GameObject GetClosestEnemy(Vector2 center, AIComponent exclude)
		{
			List<AIComponent> enemies = AIController.SharedInstance.enemies;
			GameObject result = null;
			float num = float.PositiveInfinity;
			for (int i = 0; i < enemies.Count; i++)
			{
				if (!(enemies[i] == exclude) && enemies[i].gameObject.activeInHierarchy)
				{
					Vector2 vector = new Vector2(enemies[i].transform.position.x, enemies[i].transform.position.y);
					float sqrMagnitude = (center - vector).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result = enemies[i].gameObject;
					}
				}
			}
			return result;
		}
	}
}
