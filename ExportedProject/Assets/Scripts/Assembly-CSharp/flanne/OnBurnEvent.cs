using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class OnBurnEvent : MonoBehaviour
	{
		public UnityEvent onBurn;

		private void Start()
		{
			this.AddObserver(OnInflictBurn, BurnSystem.InflictBurnEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnInflictBurn, BurnSystem.InflictBurnEvent);
		}

		private void OnInflictBurn(object sender, object args)
		{
			onBurn?.Invoke();
		}
	}
}
