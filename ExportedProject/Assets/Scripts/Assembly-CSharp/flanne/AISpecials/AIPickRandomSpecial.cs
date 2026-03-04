using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AIPickRandomSpecial", menuName = "AISpecials/AIPickRandomSpecial")]
	public class AIPickRandomSpecial : AISpecial
	{
		[SerializeField]
		private AISpecial[] specialPool;

		public override void Use(AIComponent ai, Transform target)
		{
			specialPool[Random.Range(0, specialPool.Length)].Use(ai, target);
		}
	}
}
