using System;
using UnityEngine;

namespace flanne.Player.Buffs
{
	public class AddSummonDamageToDamageMod : IDamageModifier
	{
		[SerializeField]
		private string SummonTypeID;

		[SerializeField]
		private float multiplier;

		[NonSerialized]
		private ShootingSummon _summon;

		public ValueModifier GetMod()
		{
			if (_summon == null)
			{
				ShootingSummon[] componentsInChildren = PlayerController.Instance.GetComponentsInChildren<ShootingSummon>(includeInactive: true);
				foreach (ShootingSummon shootingSummon in componentsInChildren)
				{
					if (shootingSummon.SummonTypeID == SummonTypeID)
					{
						_summon = shootingSummon;
						break;
					}
				}
			}
			return new AddValueModifier(1, (float)_summon.baseDamage * multiplier);
		}
	}
}
