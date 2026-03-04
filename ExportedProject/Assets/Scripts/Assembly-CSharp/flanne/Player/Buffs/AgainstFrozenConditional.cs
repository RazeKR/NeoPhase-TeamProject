using UnityEngine;

namespace flanne.Player.Buffs
{
	public class AgainstFrozenConditional : IBuffConditional
	{
		public bool ConditionMet(GameObject target)
		{
			return FreezeSystem.SharedInstance.IsFrozen(target);
		}
	}
}
