using UnityEngine;

namespace flanne.PowerupSystems
{
	public class DamageUpOnReload : MonoBehaviour
	{
		[SerializeField]
		private float damageBonus;

		[SerializeField]
		private float duration;

		private StatsHolder stats;

		private Ammo ammo;

		private float _timer;

		private void OnReload()
		{
			if (_timer <= 0f)
			{
				stats[StatType.BulletDamage].AddMultiplierBonus(damageBonus);
			}
			_timer = duration;
		}

		private void Start()
		{
			PlayerController componentInParent = base.transform.GetComponentInParent<PlayerController>();
			stats = componentInParent.stats;
			ammo = componentInParent.ammo;
			ammo.OnReload.AddListener(OnReload);
		}

		private void OnDestroy()
		{
			ammo.OnReload.RemoveListener(OnReload);
		}

		private void Update()
		{
			if (_timer > 0f)
			{
				_timer -= Time.deltaTime;
				if (_timer <= 0f)
				{
					stats[StatType.BulletDamage].AddMultiplierBonus(-1f * damageBonus);
				}
			}
		}
	}
}
