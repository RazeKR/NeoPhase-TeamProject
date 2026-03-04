using UnityEngine;
using flanne.PowerupSystem;

namespace flanne.PerkSystem.Actions
{
	public class ProjectileOnShootModAction : Action
	{
		[SerializeField]
		private string tagName;

		[SerializeField]
		private int additionalProjectiles;

		[SerializeField]
		private float periodicDamageFrequencyRateIncrease = 1f;

		public override void Activate(GameObject target)
		{
			ProjectileOnShoot component = GameObject.FindWithTag(tagName).GetComponent<ProjectileOnShoot>();
			component.numProjectiles += additionalProjectiles;
			component.periodicDamageFrequency /= periodicDamageFrequencyRateIncrease;
		}
	}
}
