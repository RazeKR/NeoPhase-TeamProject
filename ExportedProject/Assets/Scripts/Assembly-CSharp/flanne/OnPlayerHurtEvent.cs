using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class OnPlayerHurtEvent : MonoBehaviour
	{
		public UnityEvent onHurt;

		private PlayerHealth health;

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			health = componentInParent.playerHealth;
			health.onHurt.AddListener(OnHurt);
		}

		private void OnDestroy()
		{
			health.onHurt.RemoveListener(OnHurt);
		}

		private void OnHurt()
		{
			onHurt?.Invoke();
		}
	}
}
