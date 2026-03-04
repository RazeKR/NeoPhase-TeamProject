using UnityEngine;
using flanne.Player;

namespace flanne.PerkSystem.Actions
{
	public class AddToXPMultiplierAction : Action
	{
		[SerializeField]
		private float xpMultiplier;

		public override void Activate(GameObject target)
		{
			PlayerController.Instance.GetComponentInChildren<PlayerXP>().xpMultiplier.AddMultiplierBonus(xpMultiplier);
		}
	}
}
