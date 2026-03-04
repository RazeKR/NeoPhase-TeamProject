using System.Collections;
using UnityEngine;

namespace flanne
{
	public class SummonEgg : MonoBehaviour
	{
		[SerializeField]
		private Summon summon;

		[SerializeField]
		private ParticleSystem hatchParticles;

		[SerializeField]
		private FlashSprite hatchFlasher;

		[SerializeField]
		private SoundEffectSO soundFX;

		[SerializeField]
		private float secondsToHatch;

		private void Start()
		{
			summon.gameObject.SetActive(value: false);
			StartCoroutine(WaitToHatchCR());
		}

		private IEnumerator WaitToHatchCR()
		{
			yield return new WaitForSeconds(secondsToHatch);
			for (int i = 0; i < 3; i++)
			{
				hatchFlasher.Flash();
				yield return new WaitForSeconds(0.2f);
			}
			hatchParticles.Play();
			soundFX?.Play();
			summon.gameObject.SetActive(value: true);
			base.gameObject.SetActive(value: false);
		}
	}
}
