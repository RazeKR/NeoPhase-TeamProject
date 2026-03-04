using System.Collections;
using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AISuicideSpecial", menuName = "AISpecials/AISuicideSpecial")]
	public class AISuicideSpecial : AISpecial
	{
		[SerializeField]
		private float timeToActivate;

		[SerializeField]
		private SoundEffectSO soundFX;

		public override void Use(AIComponent ai, Transform target)
		{
			Health component = ai.GetComponent<Health>();
			if (component != null)
			{
				ai.StartCoroutine(WaitToSuicide(component));
				soundFX?.Play();
				FlashSprite component2 = ai.GetComponent<FlashSprite>();
				if (component2 != null)
				{
					ai.StartCoroutine(FlashWarning(component2));
				}
			}
			else
			{
				Debug.LogWarning("AI is missing health component: " + ai.name);
			}
		}

		private IEnumerator FlashWarning(FlashSprite flasher)
		{
			float flashTime = timeToActivate - 0.2f;
			for (float timer = 0f; timer < flashTime; timer += 0.1f)
			{
				flasher.Flash();
				yield return new WaitForSeconds(0.2f);
			}
		}

		private IEnumerator WaitToSuicide(Health health)
		{
			yield return new WaitForSeconds(timeToActivate);
			health.AutoKill();
		}
	}
}
