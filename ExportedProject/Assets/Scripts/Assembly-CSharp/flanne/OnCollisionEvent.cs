using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class OnCollisionEvent : MonoBehaviour
	{
		[SerializeField]
		private string targetTag;

		public UnityEvent onCollision;

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(targetTag))
			{
				onCollision?.Invoke();
			}
		}
	}
}
