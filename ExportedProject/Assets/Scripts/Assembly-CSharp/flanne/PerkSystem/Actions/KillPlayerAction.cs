using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class KillPlayerAction : Action
	{
		[SerializeField]
		private bool affectChampions;

		public override void Activate(GameObject target)
		{
			PlayerController.Instance.playerHealth.AutoKill();
		}
	}
}
