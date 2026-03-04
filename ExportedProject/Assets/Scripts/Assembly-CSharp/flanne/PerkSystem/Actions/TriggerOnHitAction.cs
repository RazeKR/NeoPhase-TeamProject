using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class TriggerOnHitAction : Action
	{
		public override void Activate(GameObject target)
		{
			PlayerController.Instance.gameObject.PostNotification(Projectile.ImpactEvent, target);
		}
	}
}
