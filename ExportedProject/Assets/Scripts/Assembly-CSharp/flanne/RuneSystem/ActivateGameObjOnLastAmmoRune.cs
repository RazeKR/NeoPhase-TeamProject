using System.Collections;
using UnityEngine;

namespace flanne.RuneSystem
{
	public class ActivateGameObjOnLastAmmoRune : Rune
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float percentBulletDamagePerLevel;

		[SerializeField]
		private Harmful harm;

		[SerializeField]
		private float delayAfterLastAmmo;

		[SerializeField]
		private SoundEffectSO soundFX;

		private Ammo ammo;

		protected override void Init()
		{
			ammo = player.ammo;
			ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
		}

		private void OnDestroy()
		{
			ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
		}

		private void OnAmmoChanged(int ammoAmount)
		{
			if (ammoAmount == 0)
			{
				StartCoroutine(DelayToActivateCR());
			}
		}

		private IEnumerator DelayToActivateCR()
		{
			yield return new WaitForSeconds(delayAfterLastAmmo);
			harm.damageAmount = Mathf.FloorToInt(percentBulletDamagePerLevel * player.gun.damage * (float)level);
			harm.gameObject.SetActive(value: true);
			soundFX?.Play();
		}
	}
}
