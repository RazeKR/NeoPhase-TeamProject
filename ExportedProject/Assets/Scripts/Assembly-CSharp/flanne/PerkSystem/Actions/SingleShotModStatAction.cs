using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class SingleShotModStatAction : Action
	{
		[SerializeField]
		private StatChange[] statChanges;

		private PlayerController player;

		private Gun myGun;

		private bool _isActive;

		public override void Init()
		{
			player = PlayerController.Instance;
			myGun = PlayerController.Instance.gun;
			_isActive = false;
		}

		public override void Activate(GameObject target)
		{
			if (_isActive)
			{
				return;
			}
			StatsHolder stats = player.stats;
			if (stats == null)
			{
				Debug.LogWarning("Cannot apply stat mods. No stats holder on target game object.");
				return;
			}
			StatChange[] array = statChanges;
			for (int i = 0; i < array.Length; i++)
			{
				StatChange statChange = array[i];
				if (statChange.isFlatMod)
				{
					stats[statChange.type].AddFlatBonus(statChange.flatValue);
				}
				else if (statChange.value > 0f)
				{
					stats[statChange.type].AddMultiplierBonus(statChange.value);
				}
				else if (statChange.value < 0f)
				{
					stats[statChange.type].AddMultiplierReduction(1f + statChange.value);
				}
			}
			myGun.OnShoot.AddListener(DeActivate);
			_isActive = true;
		}

		public void DeActivate()
		{
			if (!_isActive)
			{
				return;
			}
			StatsHolder stats = player.stats;
			if (stats == null)
			{
				Debug.LogWarning("Cannot apply stat mods. No stats holder on target game object.");
				return;
			}
			StatChange[] array = statChanges;
			for (int i = 0; i < array.Length; i++)
			{
				StatChange statChange = array[i];
				if (statChange.isFlatMod)
				{
					stats[statChange.type].AddFlatBonus(-1 * statChange.flatValue);
				}
				else if (statChange.value > 0f)
				{
					stats[statChange.type].AddMultiplierBonus(-1f * statChange.value);
				}
				else if (statChange.value < 0f)
				{
					stats[statChange.type].AddMultiplierReduction(1f - statChange.value);
				}
			}
			myGun.OnShoot.RemoveListener(DeActivate);
			_isActive = false;
		}
	}
}
