using System.Collections;
using UnityEngine;
using flanne.Pickups;

namespace flanne.RuneSystem
{
	public class MomentumRune : Rune
	{
		[SerializeField]
		private float moveSpeedBoostPerLevel;

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

		protected override void Init()
		{
			stats = player.stats;
			this.AddObserver(OnXPPickup, XPPickup.XPPickupEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnXPPickup, XPPickup.XPPickupEvent);
		}

		private IEnumerator StartBuffCR()
		{
			_timer = duration;
			stats[StatType.MoveSpeed].AddMultiplierBonus(moveSpeedBoostPerLevel * (float)level);
			while (_timer > 0f)
			{
				yield return null;
				_timer -= Time.deltaTime;
			}
			stats[StatType.MoveSpeed].AddMultiplierBonus(-1f * moveSpeedBoostPerLevel * (float)level);
			_timer = 0f;
		}
	}
}
