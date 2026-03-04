using System.Collections;
using UnityEngine;

namespace flanne
{
	public class TripleNextPowerup : MonoBehaviour
	{
		private bool used;

		private void OnPowerupApplied(object sender, object args)
		{
			if (!used)
			{
				used = true;
				Powerup perk = sender as Powerup;
				PlayerController.Instance.playerPerks.Equip(perk);
				PlayerController.Instance.playerPerks.Equip(perk);
				Object.Destroy(base.gameObject);
			}
		}

		private void Start()
		{
			StartCoroutine(WaitToObservePowerupCR());
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnPowerupApplied, Powerup.AppliedNotifcation);
		}

		private IEnumerator WaitToObservePowerupCR()
		{
			yield return null;
			this.AddObserver(OnPowerupApplied, Powerup.AppliedNotifcation);
		}
	}
}
