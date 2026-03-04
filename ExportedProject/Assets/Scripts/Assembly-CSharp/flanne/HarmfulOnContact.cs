using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class HarmfulOnContact : Harmful
	{
		public static string HitNotification = "HarmfulOnContact.HitNotification";

		public bool procOnHit;

		public UnityEvent onHarm;

		private PlayerController player;

		private void Start()
		{
			player = PlayerController.Instance;
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			Harm(other.gameObject);
			if (procOnHit)
			{
				player.gameObject.PostNotification(Projectile.ImpactEvent, other.gameObject);
			}
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			Harm(other.gameObject);
			if (procOnHit)
			{
				player.gameObject.PostNotification(Projectile.ImpactEvent, other.gameObject);
			}
		}

		private void Harm(GameObject gameObject)
		{
			Health component = gameObject.GetComponent<Health>();
			if (component != null)
			{
				component.HPChange(-1 * damageAmount);
				onHarm.Invoke();
				this.PostNotification(HitNotification, gameObject);
			}
		}
	}
}
