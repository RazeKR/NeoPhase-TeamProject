using UnityEngine;

namespace flanne.PowerupSystem
{
	public class SmiteHolyShieldCDR : MonoBehaviour
	{
		private PreventDamage holyShield;

		private void OnSmiteKill(object sender, object args)
		{
			holyShield.ReduceCDTimer(1f);
		}

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			holyShield = componentInParent.GetComponentInChildren<PreventDamage>();
			this.AddObserver(OnSmiteKill, SmitePassive.SmiteKillNotification);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnSmiteKill, SmitePassive.SmiteKillNotification);
		}
	}
}
