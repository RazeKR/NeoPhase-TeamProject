using UnityEngine;

namespace flanne.Player.Buffs
{
	public class AddHPToDamageMod : IDamageModifier
	{
		[SerializeField]
		private float multiplier;

		public ValueModifier GetMod()
		{
			return new AddValueModifier(1, (float)PlayerController.Instance.playerHealth.hp * multiplier);
		}
	}
}
