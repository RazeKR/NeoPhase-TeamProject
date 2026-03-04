using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class HealPlayerAction : Action
	{
		[SerializeField]
		private int healAmount = 1;

		public override void Activate(GameObject target)
		{
			PlayerController.Instance.playerHealth.Heal(healAmount);
		}
	}
}
