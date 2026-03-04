using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class OnEnemyDeathEvent : MonoBehaviour
	{
		public UnityEvent onEnemyDeath;

		private void Start()
		{
			this.AddObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDeath(object sender, object args)
		{
			onEnemyDeath?.Invoke();
		}
	}
}
