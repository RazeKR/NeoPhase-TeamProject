using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ApplyBurnEqualToBulletDamageAction : Action
	{
		[SerializeField]
		private float damageMultiplier = 1f;

		public override void Activate(GameObject target)
		{
			int burnDamage = Mathf.FloorToInt((float)CurseSystem.Instance.curseDamage * damageMultiplier);
			BurnSystem.SharedInstance.Burn(target, burnDamage);
		}
	}
}
