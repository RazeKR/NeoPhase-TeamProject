using UnityEngine;

namespace flanne
{
	public class DodgeRoller
	{
		public static string TweakDodgeNotification = "DodgeRoller.TweakDodgeNotification";

		private PlayerController player;

		private int _consecutiveHitCtr = 1;

		public DodgeRoller(PlayerController p)
		{
			player = p;
		}

		public bool Roll()
		{
			float value = player.stats[StatType.Dodge].Modify(1f) - 1f;
			value = value.NotifyModifiers(TweakDodgeNotification, player);
			float max = player.stats[StatType.DodgeCapMod].Modify(60f) / 100f;
			value = Mathf.Clamp(value, 0f, max);
			bool result;
			if (value != 0f && (float)_consecutiveHitCtr >= 1f / value)
			{
				result = true;
				_consecutiveHitCtr = 1;
			}
			else
			{
				result = Random.Range(0f, 1f) < value;
				_consecutiveHitCtr++;
			}
			return result;
		}
	}
}
