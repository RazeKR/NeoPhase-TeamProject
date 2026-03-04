using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModMagicLensBurn : Action
	{
		[SerializeField]
		private bool addBurnOnHit;

		public override void Activate(GameObject target)
		{
			BulletEnhanceSummon[] componentsInChildren = PlayerController.Instance.GetComponentsInChildren<BulletEnhanceSummon>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].addBurn = addBurnOnHit;
			}
		}
	}
}
