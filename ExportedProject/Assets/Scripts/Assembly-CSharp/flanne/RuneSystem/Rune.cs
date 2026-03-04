using UnityEngine;

namespace flanne.RuneSystem
{
	public class Rune : MonoBehaviour
	{
		protected PlayerController player;

		protected int level;

		public void Attach(PlayerController player, int level)
		{
			this.player = player;
			base.transform.SetParent(player.transform);
			this.level = level;
			Init();
		}

		protected virtual void Init()
		{
		}
	}
}
