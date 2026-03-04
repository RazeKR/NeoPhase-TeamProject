using UnityEngine;

namespace flanne.PerkSystem
{
	public abstract class Action
	{
		public abstract void Activate(GameObject target);

		public virtual void Init()
		{
		}
	}
}
