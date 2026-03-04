using System;
using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnThunderTrigger : Trigger
	{
		[SerializeField]
		private int thunderHitsToTrigger;

		[NonSerialized]
		private int _thunderCounter;

		[SerializeField]
		private bool actionTargetPlayer;

		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnThunderHit(object sender, object args)
		{
			_thunderCounter++;
			if (_thunderCounter >= thunderHitsToTrigger)
			{
				_thunderCounter = 0;
				if (actionTargetPlayer)
				{
					RaiseTrigger(PlayerController.Instance.gameObject);
					return;
				}
				GameObject target = args as GameObject;
				RaiseTrigger(target);
			}
		}
	}
}
