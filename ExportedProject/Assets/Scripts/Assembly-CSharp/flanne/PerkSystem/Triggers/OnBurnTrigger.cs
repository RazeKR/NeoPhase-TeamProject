using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnBurnTrigger : Trigger
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToTrigger = 1f;

		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnInflictBurn, BurnSystem.InflictBurnEvent);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnInflictBurn, BurnSystem.InflictBurnEvent);
		}

		private void OnInflictBurn(object sender, object args)
		{
			GameObject target = args as GameObject;
			if (Random.Range(0f, 1f) < chanceToTrigger)
			{
				RaiseTrigger(target);
			}
		}
	}
}
