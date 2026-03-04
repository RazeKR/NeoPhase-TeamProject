using UnityEngine;

namespace flanne.Player.Buffs
{
	public class AdditiveDamageMod : IDamageModifier
	{
		[SerializeField]
		private int addDamage;

		public ValueModifier GetMod()
		{
			return new AddValueModifier(1, addDamage);
		}
	}
}
