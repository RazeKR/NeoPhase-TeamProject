using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ApplyFreezeAction : Action
	{
		public override void Activate(GameObject target)
		{
			FreezeSystem.SharedInstance.Freeze(target);
		}
	}
}
