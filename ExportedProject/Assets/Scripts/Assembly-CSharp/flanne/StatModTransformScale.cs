using System;
using UnityEngine;

namespace flanne
{
	public class StatModTransformScale : MonoBehaviour
	{
		[SerializeField]
		private StatType statType;

		[SerializeField]
		private StatsHolder stats;

		[SerializeField]
		private float maxScale = float.PositiveInfinity;

		private void Start()
		{
			stats[statType].ChangedEvent += OnStatChanged;
		}

		private void OnDestroy()
		{
			stats[statType].ChangedEvent -= OnStatChanged;
		}

		private void OnStatChanged(object sender, EventArgs e)
		{
			base.transform.localScale = Vector3.one * Mathf.Min(maxScale, stats[statType].Modify(1f));
		}
	}
}
