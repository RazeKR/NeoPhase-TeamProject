using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModMagicLensBounceAction : Action
	{
		[SerializeField]
		private int additionalBounces;

		public override void Activate(GameObject target)
		{
			BulletEnhanceSummon[] componentsInChildren = PlayerController.Instance.GetComponentsInChildren<BulletEnhanceSummon>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].additionalBounces += additionalBounces;
			}
		}
	}
}
