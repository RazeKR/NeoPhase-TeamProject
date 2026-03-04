using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModCurseDamageAction : Action
	{
		[SerializeField]
		private float damageMultiMod;

		public override void Activate(GameObject target)
		{
			CurseSystem.Instance.curseDamageMultiplier += damageMultiMod;
		}
	}
}
