using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class DealBulletDamageAction : Action
	{
		[SerializeField]
		private DamageType damageType;

		[SerializeField]
		private float damageMulti = 1f;

		public override void Activate(GameObject target)
		{
			Health component = target.GetComponent<Health>();
			PlayerController instance = PlayerController.Instance;
			component.TakeDamage(damage: Mathf.FloorToInt(damageMulti * instance.gun.damage), damageType: damageType, finalMultiplier: damageMulti);
		}
	}
}
