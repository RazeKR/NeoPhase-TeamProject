using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModMagicLensMultiplierAction : Action
	{
		[SerializeField]
		private float overallMultplierMod;

		public override void Activate(GameObject target)
		{
			BulletEnhanceSummon[] componentsInChildren = PlayerController.Instance.GetComponentsInChildren<BulletEnhanceSummon>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].overallMultiplier += overallMultplierMod;
			}
		}
	}
}
