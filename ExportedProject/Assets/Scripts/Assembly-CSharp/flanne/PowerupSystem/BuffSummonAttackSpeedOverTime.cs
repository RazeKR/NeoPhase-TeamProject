using System.Collections.Generic;
using UnityEngine;

namespace flanne.PowerupSystem
{
	public class BuffSummonAttackSpeedOverTime : MonoBehaviour
	{
		[SerializeField]
		private string SummonTypeID;

		[Range(0f, 1f)]
		[SerializeField]
		private float attackSpeedBuff;

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
						summon.attackSpeedMod.AddMultiplierBonus(attackSpeedBuff);
					}
				}
				_timer -= secondsPerBuff;
			}
			_timer += Time.deltaTime;
		}
	}
}
