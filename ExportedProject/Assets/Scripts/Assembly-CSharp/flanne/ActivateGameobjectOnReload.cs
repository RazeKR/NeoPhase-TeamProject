using UnityEngine;

namespace flanne
{
	public class ActivateGameobjectOnReload : MonoBehaviour
	{
		[SerializeField]
		private GameObject obj;

		[SerializeField]
		private SoundEffectSO sfx;

		private Ammo ammo;

		private void OnReload()
		{
			obj.SetActive(value: true);
			sfx?.Play();
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
