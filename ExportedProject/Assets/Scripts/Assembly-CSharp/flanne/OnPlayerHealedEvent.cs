using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class OnPlayerHealedEvent : MonoBehaviour
	{
		public UnityEvent onHealed;

		private PlayerHealth health;

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			health = componentInParent.playerHealth;
			health.onHealed.AddListener(OnHeal);
		}

		private void OnDestroy()
		{
			health.onHealed.RemoveListener(OnHeal);
		}

		private void OnHeal()
		{
			onHealed?.Invoke();
		}
	}
}
