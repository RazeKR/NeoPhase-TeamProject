using System.Collections;
using UnityEngine;
using flanne.Pickups;

namespace flanne.PowerupSystem
{
	public class BuffOnXP : MonoBehaviour
	{
		[SerializeField]
		private float fireRateBoost;

		[SerializeField]
		private float duration;

		private StatsHolder stats;

		private float _timer;

		private void OnXPPickup(object sender, object args)
		{
			if (_timer <= 0f)
			{
				StartCoroutine(StartBuffCR());
			}
			else
			{
				_timer = duration;
			}
		}

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			stats = componentInParent.stats;
			this.AddObserver(OnXPPickup, XPPickup.XPPickupEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnXPPickup, XPPickup.XPPickupEvent);
		}

		private IEnumerator StartBuffCR()
		{
			_timer = duration;
			stats[StatType.FireRate].AddMultiplierBonus(fireRateBoost);
			while (_timer > 0f)
			{
				yield return null;
				_timer -= Time.deltaTime;
			}
			stats[StatType.FireRate].AddMultiplierBonus(-1f * fireRateBoost);
			_timer = 0f;
		}
	}
}
