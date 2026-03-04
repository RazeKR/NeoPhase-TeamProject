using System;
using UnityEngine;

namespace flanne
{
	public class StatModHealth : MonoBehaviour
	{
		[SerializeField]
		private StatsHolder stats;

		[SerializeField]
		private PlayerHealth health;

		[SerializeField]
		private int maxHP = 20;

		private void Start()
		{
			stats[StatType.MaxHP].ChangedEvent += OnStatChanged;
		}

		private void OnDestroy()
		{
			stats[StatType.MaxHP].ChangedEvent -= OnStatChanged;
		}

		private void OnStatChanged(object sender, EventArgs e)
		{
			health.maxHP = Mathf.Min(maxHP, Mathf.CeilToInt(stats[StatType.MaxHP].Modify(health.baseMaxHP)));
		}
	}
}
