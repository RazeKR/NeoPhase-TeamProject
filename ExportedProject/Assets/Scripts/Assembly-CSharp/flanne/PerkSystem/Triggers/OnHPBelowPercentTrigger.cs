using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnHPBelowPercentTrigger : Trigger
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float hpPrecentTheshold;

		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnTookDamage, Health.TookDamageEvent);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnTookDamage, Health.TookDamageEvent);
		}

		private void OnTookDamage(object sender, object args)
		{
			Health health = sender as Health;
			if ((float)health.HP / (float)health.maxHP <= hpPrecentTheshold && health.HP != 0)
			{
				RaiseTrigger(health.gameObject);
			}
		}
	}
}
