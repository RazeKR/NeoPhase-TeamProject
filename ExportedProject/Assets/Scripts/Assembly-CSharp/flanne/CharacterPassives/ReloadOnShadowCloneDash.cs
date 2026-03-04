using UnityEngine;

namespace flanne.CharacterPassives
{
	public class ReloadOnShadowCloneDash : MonoBehaviour
	{
		private ShadowClonePassive shadowClone;

		private Ammo ammo;

		private void OnShadowClone()
		{
			ammo.Reload();
		}

		private void Start()
		{
			PlayerController component = base.transform.root.GetComponent<PlayerController>();
			ammo = component.ammo;
			shadowClone = component.GetComponentInChildren<ShadowClonePassive>();
			shadowClone.onUse.AddListener(OnShadowClone);
		}

		private void OnDestroy()
		{
			shadowClone.onUse.RemoveListener(OnShadowClone);
		}
	}
}
