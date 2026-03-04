using UnityEngine;

namespace flanne.Player.Buffs
{
	public class AgainstCursedConditional : IBuffConditional
	{
		public bool ConditionMet(GameObject target)
		{
			return CurseSystem.Instance.IsCursed(target);
		}
	}
}
