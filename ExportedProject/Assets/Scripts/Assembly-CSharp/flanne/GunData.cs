using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "GunData", menuName = "GunData", order = 1)]
	public class GunData : ScriptableObject
	{
		public LocalizedString nameStringID;

		public LocalizedString descriptionStringID;

		public GameObject model;

		public Sprite icon;

		public SoundEffectSO gunshotSFX;

		public SoundEffectSO reloadSFXOverride;

		public float damage;

		public float shotCooldown;

		public int maxAmmo;

		public float reloadDuration;

		public int numOfProjectiles;

		public float spread;

		public float knockback;

		public float projectileSpeed;

		public int bounce;

		public int piercing;

		public float inaccuracy = 10f;

		public GameObject bullet;

		public bool isSummonGun;

		public bool disableManualReload;

		public List<GunEvolution> gunEvolutions;

		public string nameString => LocalizationSystem.GetLocalizedValue(nameStringID.key);

		public string description => LocalizationSystem.GetLocalizedValue(descriptionStringID.key);

		public string bulletOPTag => bullet.name;
	}
}
