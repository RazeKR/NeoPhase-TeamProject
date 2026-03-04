using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModSummonScaleAction : Action
	{
		[SerializeField]
		private string SummonTypeID;

		[SerializeField]
		private float scale = 1f;

		public override void Activate(GameObject target)
		{
			Summon[] componentsInChildren = PlayerController.Instance.GetComponentsInChildren<Summon>(includeInactive: true);
			foreach (Summon summon in componentsInChildren)
			{
				if (summon.SummonTypeID == SummonTypeID)
				{
					summon.transform.localScale *= scale;
				}
			}
		}
	}
}
