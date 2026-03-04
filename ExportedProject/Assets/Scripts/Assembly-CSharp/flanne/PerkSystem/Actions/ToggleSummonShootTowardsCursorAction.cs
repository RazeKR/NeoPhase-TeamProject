using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ToggleSummonShootTowardsCursorAction : Action
	{
		[SerializeField]
		private string SummonTypeID;

		[SerializeField]
		private bool shootTowardCursor;

		public override void Activate(GameObject target)
		{
			ShootingSummon[] componentsInChildren = PlayerController.Instance.GetComponentsInChildren<ShootingSummon>(includeInactive: true);
			foreach (ShootingSummon shootingSummon in componentsInChildren)
			{
				if (shootingSummon.SummonTypeID == SummonTypeID)
				{
					shootingSummon.targetMouse = shootTowardCursor;
				}
			}
		}
	}
}
