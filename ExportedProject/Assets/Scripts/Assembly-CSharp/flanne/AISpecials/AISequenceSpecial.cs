using System.Collections;
using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AISequenceSpecial", menuName = "AISpecials/AISequenceSpecial")]
	public class AISequenceSpecial : AISpecial
	{
		[SerializeField]
		private AISpecial[] specialSequence;

		[SerializeField]
		private float delayBetweenSpecials;

		public override void Use(AIComponent ai, Transform target)
		{
			ai.StartCoroutine(UseSpecialsCR(ai, target));
		}

		private IEnumerator UseSpecialsCR(AIComponent ai, Transform target)
		{
			for (int i = 0; i < specialSequence.Length; i++)
			{
				specialSequence[i].Use(ai, target);
				yield return new WaitForSeconds(delayBetweenSpecials);
			}
		}
	}
}
