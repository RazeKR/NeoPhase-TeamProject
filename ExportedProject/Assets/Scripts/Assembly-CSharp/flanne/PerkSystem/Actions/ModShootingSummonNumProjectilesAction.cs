using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModShootingSummonNumProjectilesAction : Action
	{
		[SerializeField]
		private string SummonTypeID;

		[SerializeField]
		private int addNumProjectiles;

		public override void Activate(GameObject target)
		{
			ShootingSummon[] componentsInChildren = PlayerController.Instance.GetComponentsInChildren<ShootingSummon>(includeInactive: true);
			foreach (ShootingSummon shootingSummon in componentsInChildren)
			{
				if (shootingSummon.SummonTypeID == SummonTypeID)
				{
					shootingSummon.numProjectiles += addNumProjectiles;
				}
			}
		}
	}
}
