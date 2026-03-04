using UnityEngine;

namespace flanne.PowerupSystem
{
	public class BuffWhenStanding : MonoBehaviour
	{
		[SerializeField]
		private float damageBoostPerTick;

		[SerializeField]
		private float secsPerTick;

		[SerializeField]
		private int maxTicks;

		private Vector3 _lastFramePos;

		private StatsHolder stats;

		private int _ticks;

		private float _timer;

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			stats = componentInParent.stats;
			_lastFramePos = base.transform.position;
		}

		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer >= secsPerTick)
			{
				_timer -= secsPerTick;
				IncrementBuff();
			}
			if (_lastFramePos != base.transform.position)
			{
				ResetBuff();
			}
			_lastFramePos = base.transform.position;
		}

		private void ResetBuff()
		{
			stats[StatType.BulletDamage].AddMultiplierBonus((float)(-1 * _ticks) * damageBoostPerTick);
			_ticks = 0;
			_timer = 0f;
		}

		private void IncrementBuff()
		{
			if (_ticks < maxTicks)
			{
				stats[StatType.BulletDamage].AddMultiplierBonus(damageBoostPerTick);
				_ticks++;
			}
		}
	}
}
