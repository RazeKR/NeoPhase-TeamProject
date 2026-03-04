using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class DamageBoostAgainstFrozen : MonoBehaviour
	{
		[SerializeField]
		private float damageBoost;

		private FreezeSystem FS;

		private void OnTweakDamge(object sender, object args)
		{
			GameObject target = (sender as Health).gameObject;
			if (FS.IsFrozen(target))
			{
				(args as List<ValueModifier>).Add(new MultValueModifier(0, 1f + damageBoost));
			}
		}

		private void Start()
		{
			this.AddObserver(OnTweakDamge, Health.TweakDamageEvent);
			FS = FreezeSystem.SharedInstance;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnTweakDamge, Health.TweakDamageEvent);
		}
	}
}
