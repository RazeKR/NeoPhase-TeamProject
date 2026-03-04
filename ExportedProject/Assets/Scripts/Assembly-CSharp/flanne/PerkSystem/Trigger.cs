using System;
using UnityEngine;

namespace flanne.PerkSystem
{
	public abstract class Trigger
	{
		public event EventHandler<GameObject> Triggered;

		public abstract void OnEquip(PlayerController player);

		public virtual void OnUnEquip(PlayerController player)
		{
		}

		protected void RaiseTrigger(GameObject target)
		{
			this.Triggered?.Invoke(this, target);
		}
	}
}
