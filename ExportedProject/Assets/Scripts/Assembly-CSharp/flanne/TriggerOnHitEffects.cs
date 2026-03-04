using UnityEngine;

namespace flanne
{
	public class TriggerOnHitEffects : MonoBehaviour
	{
		private void OnCollisionEnter2D(Collision2D other)
		{
			PlayerController.Instance.gameObject.PostNotification(Projectile.ImpactEvent, other.gameObject);
		}
	}
}
