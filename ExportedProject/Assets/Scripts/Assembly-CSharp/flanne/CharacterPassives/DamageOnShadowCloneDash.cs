using UnityEngine;

namespace flanne.CharacterPassives
{
	public class DamageOnShadowCloneDash : MonoBehaviour
	{
		[SerializeField]
		private Harmful harm;

		[SerializeField]
		private float damageMultiplier = 1f;

		private Gun gun;

		private ShadowClonePassive shadowClone;

		private void OnShadowCloneStart()
		{
			harm.damageAmount = Mathf.FloorToInt(damageMultiplier * gun.damage);
			harm.gameObject.SetActive(value: true);
		}

		private void Start()
		{
			PlayerController component = base.transform.root.GetComponent<PlayerController>();
			gun = component.gun;
			shadowClone = component.GetComponentInChildren<ShadowClonePassive>();
			shadowClone.onUse.AddListener(OnShadowCloneStart);
		}

		private void OnDestroy()
		{
			shadowClone.onUse.RemoveListener(OnShadowCloneStart);
		}
	}
}
