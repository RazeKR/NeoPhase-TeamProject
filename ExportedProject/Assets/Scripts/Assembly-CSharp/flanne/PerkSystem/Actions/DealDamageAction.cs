using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class DealDamageAction : Action
	{
		[SerializeField]
		private DamageType damageType;

		[SerializeField]
		private int baseDamage;

		[SerializeField]
		private float damageMulti = 1f;

		public override void Activate(GameObject target)
		{
			target.GetComponent<Health>().TakeDamage(damageType, baseDamage, damageMulti);
		}
	}
}
