using System;
using UnityEngine;

namespace flanne.PerkSystem
{
	[Serializable]
	public class PerkEffect
	{
		[SerializeField]
		private bool limitActivations;

		[SerializeField]
		private int limit = 1;

		[NonSerialized]
		private int _activations;

		[SerializeReference]
		private Trigger trigger;

		[SerializeReference]
		private Action action;

		[NonSerialized]
		private bool _subscribed;

		public void Equip(PlayerController player)
		{
			if (!_subscribed)
			{
				_subscribed = true;
				trigger.Triggered += OnTriggered;
				action.Init();
			}
			trigger.OnEquip(player);
		}

		public void UnEquip(PlayerController player)
		{
			_subscribed = false;
			trigger.Triggered -= OnTriggered;
			trigger.OnUnEquip(player);
		}

		private void OnTriggered(object sender, GameObject target)
		{
			if (limitActivations)
			{
				if (_activations >= limit)
				{
					return;
				}
				_activations++;
			}
			action.Activate(target);
		}
	}
}
