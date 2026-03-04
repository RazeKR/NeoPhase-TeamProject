using System.Collections.Generic;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class AddPerksToPoolAction : Action
	{
		[SerializeField]
		private List<Powerup> powerupsToAdd;

		public override void Activate(GameObject target)
		{
			PowerupGenerator.Instance.AddToPool(powerupsToAdd, 1);
		}
	}
}
