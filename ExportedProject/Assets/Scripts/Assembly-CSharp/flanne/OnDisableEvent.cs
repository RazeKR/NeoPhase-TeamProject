using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class OnDisableEvent : MonoBehaviour
	{
		[SerializeField]
		private UnityEvent onDisableEvent;

		private void OnDisable()
		{
			onDisableEvent?.Invoke();
		}
	}
}
