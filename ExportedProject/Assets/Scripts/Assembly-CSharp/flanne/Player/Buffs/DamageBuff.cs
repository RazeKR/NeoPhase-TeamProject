using System.Collections.Generic;
using UnityEngine;

namespace flanne.Player.Buffs
{
	public class DamageBuff : Buff
	{
		[SerializeField]
		private bool modAllDamageType;

		[SerializeField]
		private DamageType damageType;

		[SerializeReference]
		private IDamageModifier modifier;

		[SerializeReference]
		private IBuffConditional conditional;

		public override void OnAttach()
		{
			if (modAllDamageType)
			{
				this.AddObserver(OnTweakDamage, Health.TweakDamageEvent);
			}
			else
			{
				this.AddObserver(OnTweakDamage, $"Health.Tweak{damageType.ToString()}Damage");
			}
		}

		public override void OnUnattach()
		{
			if (modAllDamageType)
			{
				this.RemoveObserver(OnTweakDamage, Health.TweakDamageEvent);
			}
			else
			{
				this.RemoveObserver(OnTweakDamage, $"Health.Tweak{damageType.ToString()}Damage");
			}
		}

		private void OnTweakDamage(object sender, object args)
		{
			GameObject gameObject = (sender as Health).gameObject;
			if (conditional == null || conditional.ConditionMet(gameObject))
			{
				(args as List<ValueModifier>).Add(modifier.GetMod());
			}
		}
	}
}
