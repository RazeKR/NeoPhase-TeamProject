using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class IncreaseSoulHeartMaxAction : Action
	{
		[SerializeField]
		private int shpIncrease;

		public override void Activate(GameObject target)
		{
			PlayerController.Instance.playerHealth.maxSHP += shpIncrease;
		}
	}
}
