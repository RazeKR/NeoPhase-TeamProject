using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class PercentDamageAction : Action
	{
		[SerializeField]
		private DamageType damageType;

		[Range(0f, 1f)]
		[SerializeField]
		private float percentDamage;

		[Range(0f, 1f)]
		[SerializeField]
		private float championPercentDamage;

		public override void Activate(GameObject target)
		{
			Health component = target.GetComponent<Health>();
			int num = 0;
			component.TakeDamage(damage: (!target.tag.Contains("Champion") && !target.tag.Contains("Passive")) ? Mathf.FloorToInt((float)component.maxHP * percentDamage) : Mathf.FloorToInt((float)component.maxHP * championPercentDamage), damageType: damageType);
		}
	}
}
