using System.Collections;
using UnityEngine;

namespace flanne.RuneSystem
{
	public class ElementalBarrageRune : Rune
	{
		[SerializeField]
		private float bonusStatMulti;

		[SerializeField]
		private float secondsPerLevel;

		[SerializeField]
		private float cooldown;

		[SerializeField]
		private int inflictsToActivate;

		private int inflictCounter;

		private bool disableInflictGain;

		protected override void Init()
		{
			this.AddObserver(OnInflict, BurnSystem.InflictBurnEvent);
			this.AddObserver(OnInflict, FreezeSystem.InflictFreezeEvent);
			inflictCounter = 0;
			disableInflictGain = false;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnInflict, BurnSystem.InflictBurnEvent);
			this.RemoveObserver(OnInflict, FreezeSystem.InflictFreezeEvent);
		}

		private void OnInflict(object sender, object args)
		{
			if (!disableInflictGain)
			{
				inflictCounter++;
				if (inflictCounter >= inflictsToActivate)
				{
					inflictCounter = 0;
					StartCoroutine(StartBonusCR());
				}
			}
		}

		private void ActivateBonus()
		{
			player.stats[StatType.FireRate].AddMultiplierBonus(bonusStatMulti);
			player.stats[StatType.FireRate].AddMultiplierBonus(bonusStatMulti);
		}

		private void DeactivateBonus()
		{
			player.stats[StatType.FireRate].AddMultiplierBonus(-1f * bonusStatMulti);
			player.stats[StatType.FireRate].AddMultiplierBonus(-1f * bonusStatMulti);
		}

		private IEnumerator StartBonusCR()
		{
			disableInflictGain = true;
			ActivateBonus();
			yield return new WaitForSeconds(secondsPerLevel * (float)level);
			DeactivateBonus();
			StartCoroutine(WaitForCooldownCR());
		}

		private IEnumerator WaitForCooldownCR()
		{
			yield return new WaitForSeconds(cooldown);
			disableInflictGain = false;
		}
	}
}
