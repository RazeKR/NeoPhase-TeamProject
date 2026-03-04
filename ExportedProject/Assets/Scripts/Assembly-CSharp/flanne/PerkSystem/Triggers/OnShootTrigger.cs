using System;
using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnShootTrigger : Trigger
	{
		[SerializeField]
		private int shotsToTrigger;

		[NonSerialized]
		private int _shotCounter;

		public override void OnEquip(PlayerController player)
		{
			player.gun.OnShoot.AddListener(OnShoot);
		}

		public override void OnUnEquip(PlayerController player)
		{
			player.gun.OnShoot.RemoveListener(OnShoot);
		}

		private void OnShoot()
		{
			_shotCounter++;
			if (_shotCounter >= shotsToTrigger)
			{
				_shotCounter = 0;
				RaiseTrigger(PlayerController.Instance.gameObject);
			}
		}
	}
}
