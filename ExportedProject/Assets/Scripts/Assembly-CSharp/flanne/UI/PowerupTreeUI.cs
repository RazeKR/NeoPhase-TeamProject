using UnityEngine;

namespace flanne.UI
{
	public class PowerupTreeUI : DataUIBinding<PowerupTreeUIData>
	{
		[SerializeField]
		private PowerupIcon startingPowerup;

		[SerializeField]
		private PowerupIcon leftPowerup;

		[SerializeField]
		private PowerupIcon rightPowerup;

		[SerializeField]
		private PowerupIcon finalPowerup;

		public override void Refresh()
		{
			startingPowerup.data = base.data.startingPowerup;
			leftPowerup.data = base.data.leftPowerup;
			rightPowerup.data = base.data.rightPowerup;
			finalPowerup.data = base.data.finalPowerup;
		}
	}
}
