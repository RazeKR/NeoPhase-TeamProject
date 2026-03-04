using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ApplyCurse : Action
	{
		public override void Activate(GameObject target)
		{
			CurseSystem.Instance.Curse(target);
		}
	}
}
