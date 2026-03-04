using UnityEngine;

namespace flanne.RuneSystem
{
	public class AttachDamageBoostToVisionRune : Rune
	{
		[SerializeField]
		private DamageBoostInRange damageBoostPrefab;

		[SerializeField]
		private float damageBoostPerLevel;

		protected override void Init()
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag("PlayerVision");
			GameObject obj = Object.Instantiate(damageBoostPrefab.gameObject);
			obj.transform.SetParent(gameObject.transform);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			obj.SetActive(value: true);
			obj.GetComponent<DamageBoostInRange>().damageBoost = damageBoostPerLevel * (float)level;
		}
	}
}
