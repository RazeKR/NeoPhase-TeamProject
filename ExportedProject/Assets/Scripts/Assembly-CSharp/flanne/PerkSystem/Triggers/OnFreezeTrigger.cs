using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnFreezeTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnFreeze, FreezeSystem.InflictFreezeEvent);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnFreeze, FreezeSystem.InflictFreezeEvent);
		}

		private void OnFreeze(object sender, object args)
		{
			GameObject target = args as GameObject;
			RaiseTrigger(target);
		}
	}
}
