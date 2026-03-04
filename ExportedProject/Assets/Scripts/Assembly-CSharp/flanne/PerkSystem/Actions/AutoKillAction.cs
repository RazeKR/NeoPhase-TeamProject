using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class AutoKillAction : Action
	{
		[SerializeField]
		private bool affectChampions;

		public override void Activate(GameObject target)
		{
			if (affectChampions || !target.tag.Contains("Champion"))
			{
				target.GetComponent<Health>()?.AutoKill();
			}
		}
	}
}
