using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class KnockbackEnemiesAction : Action
	{
		private PlayerController player;

		public override void Init()
		{
			player = PlayerController.Instance;
		}

		public override void Activate(GameObject target)
		{
			player.KnockbackNearby();
		}
	}
}
