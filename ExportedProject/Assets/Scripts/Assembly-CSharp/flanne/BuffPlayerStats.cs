using UnityEngine;

namespace flanne
{
	public class BuffPlayerStats : MonoBehaviour
	{
		[SerializeField]
		private StatChange[] statChanges = new StatChange[0];

		private PlayerController player;

		private StatsHolder stats;

		private int _stacks;

		private void Start()
		{
			player = GetComponentInParent<PlayerController>();
			stats = player.stats;
		}

		public void AddStack()
		{
			_stacks++;
			ApplyBuff();
		}

		public void RemoveStacks()
		{
			for (int i = 0; i < _stacks; i++)
			{
				RemoveBuff();
			}
			_stacks = 0;
		}

		public void ApplyBuff()
		{
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
				if (statChange.type == StatType.MaxHP)
				{
					player.playerHealth.maxHP = Mathf.FloorToInt(stats[statChange.type].Modify(player.loadedCharacter.startHP));
				}
				if (statChange.type == StatType.CharacterSize)
				{
					player.playerSprite.transform.localScale = Vector3.one * stats[statChange.type].Modify(1f);
				}
				if (statChange.type == StatType.PickupRange)
				{
					GameObject.FindGameObjectWithTag("Pickupper").transform.localScale = Vector3.one * stats[statChange.type].Modify(1f);
				}
				if (statChange.type == StatType.VisionRange)
				{
					GameObject.FindGameObjectWithTag("PlayerVision").transform.localScale = Vector3.one * stats[statChange.type].Modify(1f);
				}
			}
		}

		public void RemoveBuff()
		{
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
				if (statChange.type == StatType.MaxHP)
				{
					player.playerHealth.maxHP = Mathf.FloorToInt(stats[statChange.type].Modify(player.loadedCharacter.startHP));
				}
				if (statChange.type == StatType.CharacterSize)
				{
					player.playerSprite.transform.localScale = Vector3.one * stats[statChange.type].Modify(1f);
				}
				if (statChange.type == StatType.PickupRange)
				{
					GameObject.FindGameObjectWithTag("Pickupper").transform.localScale = Vector3.one * stats[statChange.type].Modify(1f);
				}
				if (statChange.type == StatType.VisionRange)
				{
					GameObject.FindGameObjectWithTag("PlayerVision").transform.localScale = Vector3.one * stats[statChange.type].Modify(1f);
				}
			}
		}
	}
}
