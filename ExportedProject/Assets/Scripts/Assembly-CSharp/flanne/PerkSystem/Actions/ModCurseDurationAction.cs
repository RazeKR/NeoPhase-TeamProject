using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModCurseDurationAction : Action
	{
		[SerializeField]
		private int durationMod;

		public override void Activate(GameObject target)
		{
			CurseSystem.Instance.duration += durationMod;
		}
	}
}
