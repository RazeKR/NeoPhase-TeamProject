using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class OnPlayerReloadEvent : MonoBehaviour
	{
		public UnityEvent onReload;

		private Ammo ammo;

		private void OnReload()
		{
			onReload?.Invoke();
		}

		private void Start()
		{
			PlayerController componentInParent = base.transform.GetComponentInParent<PlayerController>();
			ammo = componentInParent.ammo;
			ammo.OnReload.AddListener(OnReload);
		}

		private void OnDestroy()
		{
			ammo.OnReload.RemoveListener(OnReload);
		}
	}
}
