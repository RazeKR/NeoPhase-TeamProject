using UnityEngine;

namespace flanne.Pickups
{
	public class HaloPickup : MonoBehaviour
	{
		public static string HaloPickupEvent = "HaloPickup.HaloPickupEvent";

		[SerializeField]
		private SoundEffectSO soundFX;

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.tag == "Player")
			{
				this.PostNotification(HaloPickupEvent, null);
				soundFX?.Play();
				Object.Destroy(base.gameObject);
			}
		}
	}
}
