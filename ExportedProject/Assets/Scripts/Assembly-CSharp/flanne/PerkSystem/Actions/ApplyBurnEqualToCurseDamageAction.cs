using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ApplyBurnEqualToCurseDamageAction : Action
	{
		[SerializeField]
		private float damageMultiplier = 1f;

		public override void Activate(GameObject target)
		{
			int burnDamage = Mathf.FloorToInt(PlayerController.Instance.gun.damage * damageMultiplier);
			BurnSystem.SharedInstance.Burn(target, burnDamage);
		}
	}
}
