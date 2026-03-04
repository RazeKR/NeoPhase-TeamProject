using System.Collections;
using UnityEngine;
using flanne.Core;

namespace flanne.RuneSystem
{
	public class SecondWindRune : Rune
	{
		[SerializeField]
		private int amountHealed;

		[SerializeField]
		private GameObject knockbackObj;

		[SerializeField]
		private ParticleSystem particleFX;

		[SerializeField]
		private SoundEffectSO soundFX;

		private void OnHealthChange(int hp)
		{
			if (hp == 1)
			{
				StartCoroutine(HealAnimationCR());
				player.playerHealth.onHealthChangedTo.RemoveListener(OnHealthChange);
			}
		}

		protected override void Init()
		{
			player.playerHealth.onHealthChangedTo.AddListener(OnHealthChange);
		}

		private IEnumerator HealAnimationCR()
		{
			player.playerHealth.isInvincible.Flip();
			PauseController.SharedInstance.Pause();
			yield return new WaitForSecondsRealtime(0.3f);
			knockbackObj.SetActive(value: true);
			soundFX?.Play();
			particleFX.Play();
			while (particleFX.isPlaying)
			{
				yield return null;
			}
			player.playerHealth.Heal(amountHealed);
			PauseController.SharedInstance.UnPause();
			yield return new WaitForSeconds(0.1f);
			player.playerHealth.isInvincible.UnFlip();
			Object.Destroy(base.gameObject);
		}
	}
}
