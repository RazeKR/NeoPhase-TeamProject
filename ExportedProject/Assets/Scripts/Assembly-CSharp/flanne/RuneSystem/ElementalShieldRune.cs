using System.Collections;
using UnityEngine;

namespace flanne.RuneSystem
{
	public class ElementalShieldRune : Rune
	{
		[SerializeField]
		private float secondsPerLevel;

		[SerializeField]
		private float cooldown;

		[SerializeField]
		private int inflictsToActivate;

		[SerializeField]
		private SoundEffectSO soundFX;

		private PlayerFlasher playerFlasher;

		private int inflictCounter;

		private bool disableInflictGain;

		protected override void Init()
		{
			this.AddObserver(OnInflict, BurnSystem.InflictBurnEvent);
			this.AddObserver(OnInflict, FreezeSystem.InflictFreezeEvent);
			inflictCounter = 0;
			disableInflictGain = false;
			playerFlasher = player.GetComponentInChildren<PlayerFlasher>();
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnInflict, BurnSystem.InflictBurnEvent);
			this.RemoveObserver(OnInflict, FreezeSystem.InflictFreezeEvent);
		}

		private void OnInflict(object sender, object args)
		{
			if (!disableInflictGain)
			{
				inflictCounter++;
				if (inflictCounter >= inflictsToActivate)
				{
					inflictCounter = 0;
					StartCoroutine(StartInvinCR());
				}
			}
		}

		private IEnumerator StartInvinCR()
		{
			disableInflictGain = true;
			player.playerHealth.isInvincible.Flip();
			playerFlasher.Flash();
			soundFX?.Play();
			yield return new WaitForSeconds(secondsPerLevel * (float)level);
			player.playerHealth.isInvincible.UnFlip();
			playerFlasher.StopFlash();
			StartCoroutine(WaitForCooldownCR());
		}

		private IEnumerator WaitForCooldownCR()
		{
			yield return new WaitForSeconds(cooldown);
			disableInflictGain = false;
		}
	}
}
