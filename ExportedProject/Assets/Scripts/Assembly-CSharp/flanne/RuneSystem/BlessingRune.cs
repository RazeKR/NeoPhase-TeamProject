using System.Collections;
using UnityEngine;

namespace flanne.RuneSystem
{
	public class BlessingRune : Rune
	{
		[SerializeField]
		private Powerup holyShieldPowerup;

		[SerializeField]
		private float cdrPerLevel;

		private PowerupGenerator powerupGenerator;

		protected override void Init()
		{
			StartCoroutine(WaitToApplyCR());
			powerupGenerator = PowerupGenerator.Instance;
		}

		private IEnumerator WaitToApplyCR()
		{
			yield return new WaitForSeconds(0.1f);
			PlayerController.Instance.playerPerks.Equip(holyShieldPowerup);
			powerupGenerator.RemoveFromPool(holyShieldPowerup);
			player.GetComponentInChildren<PreventDamage>().cooldownTime -= cdrPerLevel * (float)level;
		}
	}
}
