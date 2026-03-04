using TMPro;
using UnityEngine;
using flanne.UIExtensions;

namespace flanne.UI
{
	public class GunDescription : MenuEntryDescription<GunMenu, GunData>
	{
		[SerializeField]
		private TMP_Text nameTMP;

		[SerializeField]
		private TMP_Text descriptionTMP;

		[SerializeField]
		private TMP_Text damageTMP;

		[SerializeField]
		private TMP_Text fireRateTMP;

		[SerializeField]
		private TMP_Text projectilesTMP;

		[SerializeField]
		private TMP_Text ammoTMP;

		[SerializeField]
		private TMP_Text reloadTimeTMP;

		public override void SetProperties(GunData data)
		{
			nameTMP.text = data.nameString;
			descriptionTMP.text = data.description;
			damageTMP.text = data.damage.ToString("00");
			fireRateTMP.text = (1f / data.shotCooldown).ToString("0.0");
			projectilesTMP.text = data.numOfProjectiles.ToString("00");
			ammoTMP.text = data.maxAmmo.ToString("00");
			reloadTimeTMP.text = data.reloadDuration.ToString("0.0");
		}
	}
}
