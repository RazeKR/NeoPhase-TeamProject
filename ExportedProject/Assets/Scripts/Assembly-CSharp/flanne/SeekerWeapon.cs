using System;
using UnityEngine;

namespace flanne
{
	public class SeekerWeapon : WeaponSummon
	{
		[SerializeField]
		private SeekEnemy seeker;

		[SerializeField]
		private float baseAcceleration;

		protected override void Init()
		{
			base.summonAtkSpdMod.ChangedEvent += OnSummonAtkSpdChange;
			seeker.acceleration = base.summonAtkSpdMod.Modify(baseAcceleration);
		}

		private void OnDestroy()
		{
			base.summonAtkSpdMod.ChangedEvent -= OnSummonAtkSpdChange;
		}

		private void OnSummonAtkSpdChange(object sender, EventArgs e)
		{
			if (seeker != null)
			{
				seeker.acceleration = base.summonAtkSpdMod.Modify(baseAcceleration);
			}
		}
	}
}
