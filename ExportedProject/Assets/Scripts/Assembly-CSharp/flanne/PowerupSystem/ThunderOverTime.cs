using System.Collections.Generic;
using UnityEngine;

namespace flanne.PowerupSystem
{
	public class ThunderOverTime : MonoBehaviour
	{
		[SerializeField]
		private int baseDamage;

		[SerializeField]
		private float cooldown;

		[SerializeField]
		private int thundersPerWave;

		[SerializeField]
		private float rangeX;

		[SerializeField]
		private float rangeY;

		private ThunderGenerator TGen;

		[SerializeField]
		private Transform player;

		private float _timer;

		private void Start()
		{
			TGen = ThunderGenerator.SharedInstance;
		}

		private void Update()
		{
			_timer += Time.deltaTime;
			if (!(_timer > cooldown))
			{
				return;
			}
			_timer -= cooldown;
			for (int i = 0; i < thundersPerWave; i++)
			{
				GameObject newTarget = GetNewTarget();
				if (newTarget != null)
				{
					TGen.GenerateAt(newTarget, baseDamage);
				}
			}
		}

		private GameObject GetNewTarget()
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("Enemy");
			List<GameObject> list = new List<GameObject>();
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				if (IsWithinRange(gameObject.transform.position))
				{
					list.Add(gameObject);
				}
			}
			if (list.Count > 0)
			{
				return array[Random.Range(0, array.Length)];
			}
			return null;
		}

		private bool IsWithinRange(Vector2 pos)
		{
			if (player != null)
			{
				if (Mathf.Abs(player.position.x - pos.x) < rangeX)
				{
					return Mathf.Abs(player.position.y - pos.y) < rangeY;
				}
				return false;
			}
			return false;
		}
	}
}
