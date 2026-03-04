using UnityEngine;

namespace flanne
{
	public class DmgAndMSOnNotHurt : MonoBehaviour
	{
		[SerializeField]
		private float damageBoostPerTick;

		[SerializeField]
		private float movespeedBoostPerTick;

		[SerializeField]
		private float secsPerTick;

		[SerializeField]
		private int maxTicks;

		private SpriteTrail spriteTrail;

		private StatsHolder stats;

		private PlayerHealth health;

		private int _ticks;

		private float _timer;

		private void Start()
		{
			PlayerController componentInParent = base.transform.GetComponentInParent<PlayerController>();
			spriteTrail = componentInParent.playerSprite.GetComponentInChildren<SpriteTrail>();
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
				stats[StatType.BulletDamage].AddMultiplierBonus(damageBoostPerTick);
				stats[StatType.MoveSpeed].AddMultiplierBonus(movespeedBoostPerTick);
				if (_ticks >= maxTicks / 2)
				{
					spriteTrail.SetEnabled(enabled: true);
				}
			}
		}

		private void OnHurt()
		{
			stats[StatType.BulletDamage].AddMultiplierBonus((float)(-1 * _ticks) * damageBoostPerTick);
			stats[StatType.MoveSpeed].AddMultiplierBonus((float)(-1 * _ticks) * movespeedBoostPerTick);
			spriteTrail.SetEnabled(enabled: false);
			_ticks = 0;
			_timer = 0f;
		}
	}
}
