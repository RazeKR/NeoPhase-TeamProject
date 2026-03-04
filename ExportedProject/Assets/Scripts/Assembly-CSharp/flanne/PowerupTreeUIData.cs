using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "PowerupTreeUIData", menuName = "PowerupTreeUIData")]
	public class PowerupTreeUIData : ScriptableObject
	{
		public Powerup startingPowerup;

		public Powerup leftPowerup;

		public Powerup rightPowerup;

		public Powerup finalPowerup;
	}
}
