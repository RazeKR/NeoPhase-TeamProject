using System.Collections;
using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AIRunAtSpecial", menuName = "AISpecials/AIRunAtSpecial")]
	public class AIRunAtSpecial : AISpecial
	{
		[SerializeField]
		private float runSpeed;

		[SerializeField]
		private float windupTime;

		[SerializeField]
		private SoundEffectSO runningSFX;

		[SerializeField]
		private SoundEffectSO windupSFX;

		public override void Use(AIComponent ai, Transform target)
		{
			Vector2 direction = target.position - ai.transform.position;
			ai.StartCoroutine(RunAtCR(ai, direction));
		}

		private IEnumerator RunAtCR(AIComponent ai, Vector2 direction)
		{
			ai.animator?.SetTrigger("Windup");
			windupSFX?.Play();
			yield return new WaitForSeconds(windupTime);
			ai.animator?.SetTrigger("Special");
			ai.moveComponent.vector = direction.normalized * runSpeed;
			runningSFX?.Play();
		}
	}
}
