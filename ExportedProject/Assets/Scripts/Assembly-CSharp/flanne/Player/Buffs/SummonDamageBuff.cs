using System.Collections.Generic;
using UnityEngine;

namespace flanne.Player.Buffs
{
	public class SummonDamageBuff : Buff
	{
		[SerializeField]
		private string summonTypeID;

		[SerializeReference]
		private IDamageModifier modifier;

		public override void OnAttach()
		{
			this.AddObserver(OnTweakDamage, Summon.TweakSummonDamageNotification);
		}

		public override void OnUnattach()
		{
			this.RemoveObserver(OnTweakDamage, Summon.TweakSummonDamageNotification);
		}

		private void OnTweakDamage(object sender, object args)
		{
			Info<List<ValueModifier>, string> info = args as Info<List<ValueModifier>, string>;
			if (!(info.arg1 != summonTypeID))
			{
				info.arg0.Add(modifier.GetMod());
			}
		}
	}
}
