using UnityEngine;

namespace flanne
{
	public class NotificationOnCollision : MonoBehaviour
	{
		[SerializeField]
		private string notification;

		private void OnCollisionEnter2D(Collision2D collision)
		{
			this.PostNotification(notification, collision.gameObject);
		}
	}
}
