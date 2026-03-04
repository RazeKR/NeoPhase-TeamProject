using UnityEngine;

namespace flanne
{
	public class SetGOActiveOnReload : MonoBehaviour
	{
		[SerializeField]
		private SoundEffectSO sfx;

		[SerializeField]
		private GameObject obj;

		private Ammo ammo;

		private void OnReload()
		{
			obj.SetActive(value: true);
			sfx.Play();
		}

		private void Start()
		{
			ammo = GetComponentInParent<PlayerController>().ammo;
			ammo.OnReload.AddListener(OnReload);
		}

		private void OnDestroy()
		{
			ammo.OnReload.RemoveListener(OnReload);
		}
	}
}
