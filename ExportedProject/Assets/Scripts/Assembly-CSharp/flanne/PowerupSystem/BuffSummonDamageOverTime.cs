using System.Collections.Generic;
using UnityEngine;

namespace flanne.PowerupSystem
{
	public class BuffSummonDamageOverTime : MonoBehaviour
	{
		[SerializeField]
		private string SummonTypeID;

		[SerializeField]
		private int damageBuff;

		[SerializeField]
		private float secondsPerBuff;

		private List<ShootingSummon> _summons;

		private float _timer;

		private void Start()
		{
			ShootingSummon[] componentsInChildren = GetComponentInParent<PlayerController>().GetComponentsInChildren<ShootingSummon>(includeInactive: true);
			_summons = new List<ShootingSummon>();
			ShootingSummon[] array = componentsInChildren;
			foreach (ShootingSummon shootingSummon in array)
			{
				if (shootingSummon.SummonTypeID == SummonTypeID)
				{
					_summons.Add(shootingSummon);
				}
			}
		}

		private void Update()
		{
			if (_timer > secondsPerBuff)
			{
				foreach (ShootingSummon summon in _summons)
				{
					if (summon != null)
					{
						summon.baseDamage += damageBuff;
					}
				}
				_timer -= secondsPerBuff;
			}
			_timer += Time.deltaTime;
		}
	}
}
