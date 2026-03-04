using UnityEngine;

namespace flanne.Pickups
{
	public class DevilDealPickup : MonoBehaviour
	{
		public static string DevilDealPickupEvent = "DevilDealPickup.DevilDealPickupEvent";

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.tag == "Player" || other.tag == "MapBounds")
			{
				this.PostNotification(DevilDealPickupEvent, null);
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
