using UnityEngine;
using flanne.Pickups;

namespace flanne.PowerupSystem
{
	public class AmmoOnXP : MonoBehaviour
	{
		[SerializeField]
		private SoundEffectSO sfx;

		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToActivate;

		[SerializeField]
		private int ammoRefillAmount;

		private Ammo ammo;

		private void Start()
		{
			ammo = base.transform.parent.GetComponentInChildren<Ammo>();
			base.transform.localPosition = Vector3.zero;
			this.AddObserver(OnXPPickup, XPPickup.XPPickupEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnXPPickup, XPPickup.XPPickupEvent);
		}

		private void OnXPPickup(object sender, object args)
		{
			if (Random.Range(0f, 1f) < chanceToActivate)
			{
				if (ammo != null)
				{
					ammo.GainAmmo(ammoRefillAmount);
					sfx.Play();
				}
				else
				{
					Debug.LogWarning("No ammo component found");
				}
			}
		}
	}
}
