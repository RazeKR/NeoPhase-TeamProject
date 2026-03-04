using UnityEngine;

namespace flanne
{
	public class ExtraDamageEveryNthThunder : MonoBehaviour
	{
		[SerializeField]
		private float damageBonus;

		[SerializeField]
		private int activationThreshold;

		private int _counter;

		private ThunderGenerator TG;

		private Ammo ammo;

		private void Start()
		{
			TG = ThunderGenerator.SharedInstance;
			ammo = base.transform.parent.GetComponentInChildren<Ammo>();
			this.AddObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnThunderHit(object sender, object args)
		{
			_counter++;
			if (_counter == activationThreshold - 1)
			{
				TG.damageMod.AddMultiplierBonus(damageBonus);
			}
			else if (_counter >= activationThreshold)
			{
				_counter = 0;
				TG.damageMod.AddMultiplierBonus(-1f * damageBonus);
				ammo.GainAmmo();
			}
		}
	}
}
