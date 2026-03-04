using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModThunderAreaAction : Action
	{
		[SerializeField]
		private float thunderAOEMod;

		public override void Activate(GameObject target)
		{
			ThunderGenerator.SharedInstance.sizeMultiplier += thunderAOEMod;
		}
	}
}
