using UnityEngine;

namespace flanne.Player.Buffs
{
	public class MultiplierDamageMod : IDamageModifier
	{
		[SerializeField]
		private float multiplier = 1f;

		public ValueModifier GetMod()
		{
			return new MultValueModifier(0, multiplier);
		}
	}
}
