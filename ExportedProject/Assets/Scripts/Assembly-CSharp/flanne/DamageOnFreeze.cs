using UnityEngine;

namespace flanne
{
	public class DamageOnFreeze : MonoBehaviour
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float percentDamage;

		[Range(0f, 1f)]
		[SerializeField]
		private float championPercentDamage;

		private void Start()
		{
			this.AddObserver(OnFreeze, FreezeSystem.InflictFreezeEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnFreeze, FreezeSystem.InflictFreezeEvent);
		}

		private void OnFreeze(object sender, object args)
		{
			GameObject obj = args as GameObject;
			Health component = obj.GetComponent<Health>();
			int change = ((!obj.tag.Contains("Champion")) ? (-1 * Mathf.FloorToInt((float)component.maxHP * percentDamage)) : (-1 * Mathf.FloorToInt((float)component.maxHP * championPercentDamage)));
			component.HPChange(change);
		}
	}
}
