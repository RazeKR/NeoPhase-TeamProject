using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ShatterFrozenAction : Action
	{
		[SerializeField]
		private GameObject shatterPrefab;

		[Range(0f, 1f)]
		[SerializeField]
		private float shatterPercentDamage;

		[SerializeField]
		private SoundEffectSO soundFX;

		public override void Activate(GameObject target)
		{
			ObjectPooler sharedInstance = ObjectPooler.SharedInstance;
			sharedInstance.AddObject(shatterPrefab.name, shatterPrefab, 25);
			Health component = target.GetComponent<Health>();
			if (FreezeSystem.SharedInstance.IsFrozen(component.gameObject))
			{
				GameObject pooledObject = sharedInstance.GetPooledObject(shatterPrefab.name);
				pooledObject.transform.position = component.transform.position;
				pooledObject.GetComponent<Harmful>().damageAmount = Mathf.FloorToInt((float)component.maxHP * shatterPercentDamage);
				pooledObject.SetActive(value: true);
				soundFX?.Play();
			}
		}
	}
}
