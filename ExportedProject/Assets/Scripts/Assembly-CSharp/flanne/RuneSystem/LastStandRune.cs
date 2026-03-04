using System.Collections;
using UnityEngine;

namespace flanne.RuneSystem
{
	public class LastStandRune : Rune
	{
		[SerializeField]
		private float invulnDurationPerLevel;

		[SerializeField]
		private GameObject knockbackObj;

		[SerializeField]
		private SoundEffectSO soundFX;

		private PlayerFlasher playerFlasher;

		private bool active;

		private void OnHealthChange(int hp)
		{
			if (hp == 1 && !active)
			{
				StartCoroutine(InvulnCR());
			}
		}

		protected override void Init()
		{
			player.playerHealth.onHealthChangedTo.AddListener(OnHealthChange);
			active = false;
			playerFlasher = player.GetComponentInChildren<PlayerFlasher>();
		}

		private void OnDestroy()
		{
			player.playerHealth.onHealthChangedTo.RemoveListener(OnHealthChange);
		}

		private IEnumerator InvulnCR()
		{
			active = true;
			player.playerHealth.isInvincible.Flip();
			playerFlasher.Flash();
			knockbackObj.SetActive(value: true);
			soundFX?.Play();
			yield return new WaitForSeconds(invulnDurationPerLevel * (float)level);
			active = false;
			player.playerHealth.isInvincible.UnFlip();
			playerFlasher.StopFlash();
			Object.Destroy(base.gameObject);
		}
	}
}
