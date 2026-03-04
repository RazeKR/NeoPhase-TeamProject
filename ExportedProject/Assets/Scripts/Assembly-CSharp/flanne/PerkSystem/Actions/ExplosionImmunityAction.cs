using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ExplosionImmunityAction : Action
	{
		public override void Activate(GameObject target)
		{
			PlayerController.Instance.playerHealth.RemoveVulnerability("HarmfulToPlayer");
		}
	}
}
