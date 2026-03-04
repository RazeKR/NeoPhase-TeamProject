using UnityEngine;

namespace flanne.Player.Buffs
{
	public interface IBuffConditional
	{
		bool ConditionMet(GameObject target);
	}
}
