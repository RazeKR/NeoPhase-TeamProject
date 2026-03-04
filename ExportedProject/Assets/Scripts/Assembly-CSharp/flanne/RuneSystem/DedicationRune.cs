using UnityEngine;

namespace flanne.RuneSystem
{
	public class DedicationRune : Rune
	{
		[SerializeField]
		private float summonStatBuffPerLevel;

		private bool active;

		private float summonStatBuff => summonStatBuffPerLevel * (float)level;

		private void OnSummonChanged(object sender, object args)
		{
			CheckSummons();
		}

		protected override void Init()
		{
			CheckSummons();
			this.AddObserver(OnSummonChanged, GuardianRune.SummonDestroyedNotification);
			this.AddObserver(OnSummonChanged, Powerup.AppliedNotifcation);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnSummonChanged, GuardianRune.SummonDestroyedNotification);
			this.RemoveObserver(OnSummonChanged, Powerup.AppliedNotifcation);
		}

		private void CheckSummons()
		{
			if (Object.FindObjectsOfType<Summon>().Length == 1)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}

		private void Activate()
		{
			if (!active)
			{
				active = true;
				player.stats[StatType.SummonDamage].AddMultiplierBonus(summonStatBuff);
				player.stats[StatType.SummonAttackSpeed].AddMultiplierBonus(summonStatBuff);
			}
		}

		private void Deactivate()
		{
			if (active)
			{
				active = false;
				player.stats[StatType.SummonDamage].AddMultiplierBonus(-1f * summonStatBuff);
				player.stats[StatType.SummonAttackSpeed].AddMultiplierBonus(-1f * summonStatBuff);
			}
		}
	}
}
