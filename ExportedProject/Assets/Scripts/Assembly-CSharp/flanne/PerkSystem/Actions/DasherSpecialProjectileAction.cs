using UnityEngine;
using flanne.CharacterPassives;

namespace flanne.PerkSystem.Actions
{
	public class DasherSpecialProjectileAction : Action
	{
		public override void Activate(GameObject target)
		{
			PlayerController.Instance.GetComponentInChildren<DasherSpecial>().projectiles++;
		}
	}
}
