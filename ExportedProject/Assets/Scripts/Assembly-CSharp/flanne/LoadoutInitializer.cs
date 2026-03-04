using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using flanne.RuneSystem;

namespace flanne
{
	public class LoadoutInitializer : MonoBehaviour
	{
		[SerializeField]
		private CharacterData defaultCharacter;

		[SerializeField]
		private PlayerController player;

		[SerializeField]
		private Animator playerAnimator;

		[SerializeField]
		private PlayerHealth playerHealth;

		[SerializeField]
		private PowerupGenerator powerupGenerator;

		[SerializeField]
		private Gun gun;

		[SerializeField]
		private Image characterIcon;

		[SerializeField]
		private Image gunIcon;

		private void Start()
		{
			CharacterData characterSelection = Loadout.CharacterSelection;
			if (characterSelection == null)
			{
				characterSelection = defaultCharacter;
			}
			playerAnimator.runtimeAnimatorController = characterSelection.animController;
			playerHealth.baseMaxHP = characterSelection.startHP;
			if (characterSelection.passivePrefab != null)
			{
				GameObject obj = Object.Instantiate(characterSelection.passivePrefab);
				obj.transform.SetParent(player.transform.root);
				obj.transform.localPosition = Vector3.zero;
			}
			player.loadedCharacter = characterSelection;
			powerupGenerator.SetCharacterPowerupPool(characterSelection.exclusivePowerups);
			characterIcon.sprite = characterSelection.icon;
			GunData gunSelection = Loadout.GunSelection;
			gun.LoadGun(gunSelection);
			if (gunSelection != null)
			{
				gunIcon.sprite = gunSelection.icon;
			}
			MapData mapData = SelectedMap.MapData;
			if (!(mapData != null) || mapData.runesDisabled)
			{
				return;
			}
			List<RuneData> runeSelection = Loadout.RuneSelection;
			if (runeSelection == null)
			{
				return;
			}
			foreach (RuneData item in runeSelection)
			{
				item.Apply(player);
			}
		}
	}
}
