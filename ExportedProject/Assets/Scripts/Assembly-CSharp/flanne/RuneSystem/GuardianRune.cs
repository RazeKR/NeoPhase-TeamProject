using System.Collections;
using UnityEngine;
using flanne.Core;

namespace flanne.RuneSystem
{
	public class GuardianRune : Rune
	{
		public static string SummonDestroyedNotification = "GuardianRune.SummonDestroyedNotification";

		[SerializeField]
		private GameObject summonDeathFXObj;

		[SerializeField]
		private GameObject knockbackObj;

		[SerializeField]
		private SoundEffectSO soundFX;

		private bool active;

		private int counter;

		[SerializeField]
		private int cap => level;

		private void OnPreventDamage()
		{
			if (active)
			{
				Summon[] array = Object.FindObjectsOfType<Summon>();
				counter++;
				if (counter < cap && array.Length != 0)
				{
					int num = Random.Range(0, array.Length);
					StartCoroutine(KillSummonCR(array[num]));
					player.playerHealth.isProtected = true;
				}
			}
		}

		private void OnHealthChange(int hp)
		{
			if (counter >= cap)
			{
				return;
			}
			if (hp == 1)
			{
				if (Object.FindObjectsOfType<Summon>().Length != 0)
				{
					active = true;
					StartCoroutine(WaitToActivateCR());
				}
			}
			else if (active)
			{
				player.playerHealth.isProtected = false;
				active = false;
			}
		}

		protected override void Init()
		{
			player.playerHealth.onHealthChangedTo.AddListener(OnHealthChange);
			player.playerHealth.onDamagePrevented.AddListener(OnPreventDamage);
		}

		private void OnDestroy()
		{
			player.playerHealth.onHealthChangedTo.RemoveListener(OnHealthChange);
			player.playerHealth.onDamagePrevented.RemoveListener(OnPreventDamage);
		}

		private IEnumerator WaitToActivateCR()
		{
			yield return new WaitForSeconds(0.1f);
			player.playerHealth.isProtected = true;
		}

		private IEnumerator KillSummonCR(Summon summon)
		{
			PauseController.SharedInstance.Pause();
			yield return new WaitForSecondsRealtime(0.3f);
			summonDeathFXObj.transform.position = summon.transform.position;
			summonDeathFXObj.SetActive(value: true);
			Object.Destroy(summon.gameObject);
			soundFX?.Play();
			yield return new WaitForSecondsRealtime(0.4f);
			summonDeathFXObj.SetActive(value: false);
			yield return new WaitForSecondsRealtime(0.3f);
			knockbackObj.SetActive(value: true);
			PauseController.SharedInstance.UnPause();
			this.PostNotification(SummonDestroyedNotification, null);
		}
	}
}
