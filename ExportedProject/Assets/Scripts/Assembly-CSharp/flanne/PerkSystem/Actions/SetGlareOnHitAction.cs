using UnityEngine;
using flanne.PowerupSystem;

namespace flanne.PerkSystem.Actions
{
	public class SetGlareOnHitAction : Action
	{
		public override void Activate(GameObject target)
		{
			AttachVisionDamage[] componentsInChildren = target.GetComponentsInChildren<AttachVisionDamage>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].visionDamage.triggerOnHit = true;
			}
		}
	}
}
