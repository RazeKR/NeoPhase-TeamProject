using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "PowerupProfile", menuName = "PowerupProfile", order = 2)]
	public class PowerupPoolProfile : ScriptableObject
	{
		public List<Powerup> powerupPool;
	}
}
