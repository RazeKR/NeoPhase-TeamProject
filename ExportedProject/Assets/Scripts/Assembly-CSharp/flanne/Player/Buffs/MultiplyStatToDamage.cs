using UnityEngine;

namespace flanne.Player.Buffs
{
	public class MultiplyStatToDamage : IDamageModifier
	{
		[SerializeField]
		private StatType statType;

		public ValueModifier GetMod()
		{
			float toMultiply = PlayerController.Instance.stats[statType].Modify(1f);
			return new MultValueModifier(1, toMultiply);
		}
	}
}
