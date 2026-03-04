using UnityEngine;
using flanne.Player;

namespace flanne.PerkSystem.Actions
{
	public class ApplyPlayerBuffAction : Action
	{
		[SerializeReference]
		private Buff buff;

		public override void Activate(GameObject target)
		{
			PlayerBuffs componentInChildren = target.GetComponentInChildren<PlayerBuffs>();
			if (componentInChildren == null)
			{
				Debug.LogWarning("No PlayerBuff component found on target.");
			}
			componentInChildren.Add(buff);
		}
	}
}
