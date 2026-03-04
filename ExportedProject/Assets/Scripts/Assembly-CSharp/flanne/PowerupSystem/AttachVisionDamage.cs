using UnityEngine;

namespace flanne.PowerupSystem
{
	public class AttachVisionDamage : MonoBehaviour
	{
		[SerializeField]
		private PersistentHarm visionDamagePrefab;

		public PersistentHarm visionDamage { get; private set; }

		private void Start()
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag("PlayerVision");
			PersistentHarm componentInChildren = gameObject.GetComponentInChildren<PersistentHarm>();
			GameObject gameObject2 = Object.Instantiate(visionDamagePrefab.gameObject);
			gameObject2.transform.SetParent(gameObject.transform);
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.SetActive(value: true);
			visionDamage = gameObject2.GetComponent<PersistentHarm>();
			if (componentInChildren != null)
			{
				visionDamage.damageAmount = componentInChildren.damageAmount;
				visionDamage.tickRate = componentInChildren.tickRate;
			}
		}
	}
}
