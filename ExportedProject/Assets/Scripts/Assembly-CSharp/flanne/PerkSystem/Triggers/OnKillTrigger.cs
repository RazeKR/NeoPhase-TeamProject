using System;
using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnKillTrigger : Trigger
	{
		[SerializeField]
		private bool anyDamageType;

		[SerializeField]
		private DamageType damageType;

		[SerializeField]
		private int killsToTrigger;

		[SerializeField]
		private bool actionTargetPlayer;

		[NonSerialized]
		private int _killCounter;

		public override void OnEquip(PlayerController player)
		{
			if (anyDamageType)
			{
				this.AddObserver(OnKill, Health.DeathEvent);
			}
			else
			{
				this.AddObserver(OnKill, $"Health.{damageType.ToString()}DamageKill");
			}
		}

		public override void OnUnEquip(PlayerController player)
		{
			if (anyDamageType)
			{
				this.RemoveObserver(OnKill, Health.DeathEvent);
			}
			else
			{
				this.RemoveObserver(OnKill, $"Health.{damageType.ToString()}DamageKill");
			}
		}

		private void OnKill(object sender, object args)
		{
			_killCounter++;
			if (_killCounter >= killsToTrigger)
			{
				_killCounter = 0;
				if (actionTargetPlayer)
				{
					RaiseTrigger(PlayerController.Instance.gameObject);
					return;
				}
				GameObject gameObject = (sender as Health).gameObject;
				RaiseTrigger(gameObject);
			}
		}
	}
}
