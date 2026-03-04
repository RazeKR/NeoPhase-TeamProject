using System;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class DamageBoostInRange : MonoBehaviour
	{
		[NonSerialized]
		public float damageBoost;

		[SerializeField]
		private string hitTag;

		private List<Health> _targetsInRange;

		private void OnTweakDamage(object sender, object args)
		{
			(args as List<ValueModifier>).Add(new MultValueModifier(0, 1f + damageBoost));
		}

		private void Start()
		{
			_targetsInRange = new List<Health>();
		}

		private void OnDestroy()
		{
			foreach (Health item in _targetsInRange)
			{
				this.RemoveObserver(OnTweakDamage, Health.TweakDamageEvent, item);
			}
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(hitTag))
			{
				Health component = other.gameObject.GetComponent<Health>();
				if (component != null)
				{
					this.AddObserver(OnTweakDamage, Health.TweakDamageEvent, component);
					_targetsInRange.Add(component);
				}
			}
		}

		private void OnCollisionExit2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(hitTag))
			{
				Health component = other.gameObject.GetComponent<Health>();
				if (component != null)
				{
					this.RemoveObserver(OnTweakDamage, Health.TweakDamageEvent, component);
					_targetsInRange.Remove(component);
				}
			}
		}
	}
}
