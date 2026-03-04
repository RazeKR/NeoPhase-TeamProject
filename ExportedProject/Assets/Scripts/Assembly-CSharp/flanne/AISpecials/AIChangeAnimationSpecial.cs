using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AIChangeAnimationSpecial", menuName = "AISpecials/AIChangeAnimationSpecial")]
	public class AIChangeAnimationSpecial : AISpecial
	{
		public override void Use(AIComponent ai, Transform target)
		{
			ai.animator?.SetTrigger("Special");
		}
	}
}
