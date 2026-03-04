using UnityEngine;

namespace flanne.Player.Buffs
{
	public class MultiplyBaseProjToDamage : IDamageModifier
	{
		[SerializeField]
		private float multiplier = 1f;

		public ValueModifier GetMod()
		{
			PlayerController instance = PlayerController.Instance;
			float toMultiply = (float)(instance.stats[StatType.Projectiles].FlatBonus + instance.gun.gunData.numOfProjectiles) * multiplier;
			return new MultValueModifier(1, toMultiply);
		}
	}
}
