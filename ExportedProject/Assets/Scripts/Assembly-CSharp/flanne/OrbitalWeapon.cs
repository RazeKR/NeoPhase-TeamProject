using System;
using UnityEngine;

namespace flanne
{
	public class OrbitalWeapon : WeaponSummon
	{
		[SerializeField]
		private Orbital orbital;

		[SerializeField]
		private float baseRotationSpeed;

		protected override void Init()
		{
			base.summonAtkSpdMod.ChangedEvent += OnSummonAtkSpdChange;
			orbital.rotationSpeed = base.summonAtkSpdMod.Modify(baseRotationSpeed);
		}

		private void OnDestroy()
		{
			base.summonAtkSpdMod.ChangedEvent -= OnSummonAtkSpdChange;
		}

		private void OnSummonAtkSpdChange(object sender, EventArgs e)
		{
			if (orbital != null)
			{
				orbital.rotationSpeed = base.summonAtkSpdMod.Modify(baseRotationSpeed);
			}
		}
	}
}
