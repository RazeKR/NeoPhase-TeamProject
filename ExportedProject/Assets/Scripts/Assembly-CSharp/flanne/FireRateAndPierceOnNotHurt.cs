using UnityEngine;

namespace flanne
{
	public class FireRateAndPierceOnNotHurt : MonoBehaviour
	{
		[SerializeField]
		private float fireRateBoostPerTick;

		[SerializeField]
		private int pierceBoostPerTick;

		[SerializeField]
		private float secsPerTick;

		[SerializeField]
		private int maxTicks;

		private StatsHolder stats;

		private PlayerHealth health;

		private int _ticks;

		private float _timer;

		private void Start()
		{
			PlayerController componentInParent = base.transform.GetComponentInParent<PlayerController>();
			stats = componentInParent.stats;
			health = componentInParent.playerHealth;
			health.onHurt.AddListener(OnHurt);
		}

		private void OnDestroy()
		{
			health.onHurt.RemoveListener(OnHurt);
		}

		private void Update()
		{
			if (_ticks < maxTicks)
			{
				_timer += Time.deltaTime;
			}
			if (_timer >= secsPerTick)
			{
				_timer -= secsPerTick;
				_ticks++;
				stats[StatType.FireRate].AddMultiplierBonus(fireRateBoostPerTick);
				stats[StatType.Piercing].AddFlatBonus(pierceBoostPerTick);
			}
		}

		private void OnHurt()
		{
			stats[StatType.FireRate].AddMultiplierBonus((float)(-1 * _ticks) * fireRateBoostPerTick);
			stats[StatType.Piercing].AddFlatBonus(-1 * _ticks * pierceBoostPerTick);
			_ticks = 0;
			_timer = 0f;
		}
	}
}
