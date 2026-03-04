using System.Collections;
using UnityEngine;

namespace flanne.PowerupSystem
{
	public class FireRateBuffOnGainAmmo : MonoBehaviour
	{
		[SerializeField]
		private float duration;

		[SerializeField]
		private float buffAmount;

		private StatsHolder playerStats;

		private Ammo ammo;

		private IEnumerator _buffCoroutine;

		private float _timer;

		private void OnAmmoGained()
		{
			if (_buffCoroutine == null)
			{
				_buffCoroutine = BuffCR();
				StartCoroutine(_buffCoroutine);
			}
			else
			{
				_timer = 0f;
			}
		}

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			playerStats = componentInParent.stats;
			ammo = componentInParent.ammo;
			ammo.OnAmmoGained.AddListener(OnAmmoGained);
		}

		private void OnDestroy()
		{
			ammo.OnAmmoGained.RemoveListener(OnAmmoGained);
		}

		private IEnumerator BuffCR()
		{
			playerStats[StatType.FireRate].AddMultiplierBonus(buffAmount);
			while (_timer < duration)
			{
				yield return null;
				_timer += Time.deltaTime;
			}
			playerStats[StatType.FireRate].AddMultiplierBonus(-1f * buffAmount);
			_buffCoroutine = null;
		}
	}
}
