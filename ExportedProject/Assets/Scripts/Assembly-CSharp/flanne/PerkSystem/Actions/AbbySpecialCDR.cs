using UnityEngine;
using flanne.CharacterPassives;

namespace flanne.PerkSystem.Actions
{
	public class AbbySpecialCDR : Action
	{
		[SerializeField]
		private float shotCDR;

		public override void Activate(GameObject target)
		{
			DumpAmmoPassive componentInChildren = target.GetComponentInChildren<DumpAmmoPassive>();
			if (componentInChildren != null)
			{
				componentInChildren.shotCDMultiplier /= 1f + shotCDR;
			}
			else
			{
				Debug.LogWarning("Abby's special not found.");
			}
		}
	}
}
