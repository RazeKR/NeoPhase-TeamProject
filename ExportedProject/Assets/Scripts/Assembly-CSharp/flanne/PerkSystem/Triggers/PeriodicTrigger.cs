using System.Collections;
using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class PeriodicTrigger : Trigger
	{
		[SerializeField]
		private float triggerIntervalSeconds;

		public override void OnEquip(PlayerController player)
		{
			player.StartCoroutine(PeriodicTriggerCR(player.gameObject));
		}

		private IEnumerator PeriodicTriggerCR(GameObject target)
		{
			while (true)
			{
				yield return new WaitForSeconds(triggerIntervalSeconds);
				RaiseTrigger(target);
			}
		}
	}
}
