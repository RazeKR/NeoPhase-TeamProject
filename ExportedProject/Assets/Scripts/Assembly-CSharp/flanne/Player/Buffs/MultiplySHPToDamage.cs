using UnityEngine;

namespace flanne.Player.Buffs
{
	public class MultiplySHPToDamage : IDamageModifier
	{
		[SerializeField]
		private float multiplierPerSHP = 0.1f;

		public ValueModifier GetMod()
		{
			float num = (float)PlayerController.Instance.playerHealth.shp * multiplierPerSHP;
			num += 1f;
			return new MultValueModifier(1, num);
		}
	}
}
