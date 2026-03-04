using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ModStatAction : Action
	{
		[SerializeField]
		private StatChange[] statChanges;

		public override void Activate(GameObject target)
		{
			StatsHolder componentInChildren = target.GetComponentInChildren<StatsHolder>();
			if (componentInChildren == null)
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
					componentInChildren[statChange.type].AddFlatBonus(statChange.flatValue);
				}
				else if (statChange.value > 0f)
				{
					componentInChildren[statChange.type].AddMultiplierBonus(statChange.value);
				}
				else if (statChange.value < 0f)
				{
					componentInChildren[statChange.type].AddMultiplierReduction(1f + statChange.value);
				}
			}
		}
	}
}
