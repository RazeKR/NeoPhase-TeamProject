using UnityEngine;

namespace flanne.RuneSystem
{
	public class SoulAttunedRune : Rune
	{
		[SerializeField]
		private int shpMaxPerLevel;

		protected override void Init()
		{
			PlayerController.Instance.playerHealth.maxSHP += shpMaxPerLevel * level;
		}
	}
}
