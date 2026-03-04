using System.Collections.Generic;
using UnityEngine;
using flanne.PerkSystem;

namespace flanne
{
	[CreateAssetMenu(fileName = "Perk", menuName = "Perk")]
	public class Powerup : ScriptableObject
	{
		public static string AppliedNotifcation = "Powerup.AppliedNotifcation";

		public Sprite icon;

		[SerializeField]
		private LocalizedString nameStrID;

		[SerializeField]
		private LocalizedString desStrID;

		public bool anyPrereqFulfill;

		public List<Powerup> prereqs;

		public PowerupTreeUIData powerupTreeUIData;

		[SerializeField]
		private StatChange[] statChanges;

		[SerializeField]
		private PerkEffect[] effects;

		[SerializeField]
		private PerkEffect[] stackedEffects;

		public string nameString => LocalizationSystem.GetLocalizedValue(nameStrID.key);

		public string description
		{
			get
			{
				string text = string.Empty;
				StatChange[] array = statChanges;
				for (int i = 0; i < array.Length; i++)
				{
					StatChange statChange = array[i];
					text = text + LocalizationSystem.GetLocalizedValue(StatLabels.Labels[statChange.type]) + " ";
					if (statChange.isFlatMod)
					{
						if (statChange.flatValue > 0)
						{
							text = text + "<color=#f5d6c1>+" + statChange.flatValue + "</color><br>";
						}
						else if (statChange.flatValue < 0)
						{
							text = text + "<color=#fd5161>" + statChange.flatValue + "</color><br>";
						}
					}
					else if (statChange.value > 0f)
					{
						text = text + "<color=#f5d6c1>+" + Mathf.FloorToInt(statChange.value * 100f) + "%</color><br>";
					}
					else if (statChange.value < 0f)
					{
						text = text + "<color=#fd5161>" + Mathf.FloorToInt(statChange.value * 100f) + "%</color><br>";
					}
				}
				return text + LocalizationSystem.GetLocalizedValue(desStrID.key);
			}
		}

		public void Apply(PlayerController player)
		{
			this.PostNotification(AppliedNotifcation);
			player.onDestroyed.AddListener(delegate
			{
				OnPlayerDestroyed(player, effects);
			});
			ApplyStats(player);
			PerkEffect[] array = effects;
			for (int num = 0; num < array.Length; num++)
			{
				array[num].Equip(player);
			}
		}

		public void ApplyStack(PlayerController player)
		{
			this.PostNotification(AppliedNotifcation);
			ApplyStats(player);
			PerkEffect[] array;
			if (stackedEffects.Length == 0)
			{
				player.onDestroyed.AddListener(delegate
				{
					OnPlayerDestroyed(player, effects);
				});
				array = effects;
			}
			else
			{
				player.onDestroyed.AddListener(delegate
				{
					OnPlayerDestroyed(player, stackedEffects);
				});
				array = stackedEffects;
			}
			PerkEffect[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				array2[num].Equip(player);
			}
		}

		private void ApplyStats(PlayerController player)
		{
			StatsHolder componentInChildren = player.GetComponentInChildren<StatsHolder>();
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

		private void OnPlayerDestroyed(PlayerController player, PerkEffect[] equippedEffects)
		{
			player.onDestroyed.RemoveListener(delegate
			{
				OnPlayerDestroyed(player, equippedEffects);
			});
			PerkEffect[] array = equippedEffects;
			for (int num = 0; num < array.Length; num++)
			{
				array[num].UnEquip(player);
			}
		}
	}
}
