using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class DamageBoostAgainstCursed : MonoBehaviour
	{
		[SerializeField]
		private float damageBoost;

		private CurseSystem CS;

		private void OnTweakDamge(object sender, object args)
		{
			GameObject target = (sender as Health).gameObject;
			if (CS.IsCursed(target))
			{
				(args as List<ValueModifier>).Add(new MultValueModifier(0, 1f + damageBoost));
			}
		}

		private void Start()
		{
			this.AddObserver(OnTweakDamge, Health.TweakDamageEvent);
			CS = CurseSystem.Instance;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnTweakDamge, Health.TweakDamageEvent);
		}
	}
}
