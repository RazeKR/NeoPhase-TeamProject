using System.Collections;
using UnityEngine;

namespace flanne.Player.Buffs
{
	public class TemporaryStatBuff : Buff
	{
		[SerializeField]
		private StatChange[] statChanges;

		[SerializeField]
		private float duration;

		public override void OnAttach()
		{
			PlayerController instance = PlayerController.Instance;
			StatsHolder stats = instance.stats;
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
			instance.StartCoroutine(WaitDurationCR());
		}

		public override void OnUnattach()
		{
			StatsHolder stats = PlayerController.Instance.stats;
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
		}

		private IEnumerator WaitDurationCR()
		{
			yield return new WaitForSeconds(duration);
			owner.Remove(this);
		}
	}
}
