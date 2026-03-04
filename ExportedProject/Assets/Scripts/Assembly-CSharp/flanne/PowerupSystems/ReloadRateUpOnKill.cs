using UnityEngine;

namespace flanne.PowerupSystems
{
	public class ReloadRateUpOnKill : MonoBehaviour
	{
		[SerializeField]
		private float bonusPerStack;

		private StatsHolder stats;

		private Ammo ammo;

		private int _stacks;

		private void OnDeath(object sender, object args)
		{
			if ((sender as Health).gameObject.tag == "Enemy")
			{
				stats[StatType.ReloadRate].AddMultiplierBonus(bonusPerStack);
				_stacks++;
			}
		}

		private void OnReload()
		{
			stats[StatType.ReloadRate].AddMultiplierBonus((float)(-1 * _stacks) * bonusPerStack);
			_stacks = 0;
		}

		private void Start()
		{
			PlayerController componentInParent = base.transform.GetComponentInParent<PlayerController>();
			stats = componentInParent.stats;
			ammo = componentInParent.ammo;
			ammo.OnReload.AddListener(OnReload);
			this.AddObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDestroy()
		{
			ammo.OnReload.RemoveListener(OnReload);
			this.RemoveObserver(OnDeath, Health.DeathEvent);
		}
	}
}
