using System.Collections.Generic;
using UnityEngine;

namespace flanne.Player.Buffs
{
	public class ModSummonAttackCDByFireRateMod : Buff
	{
		[SerializeField]
		private string summonTypeID;

		private PlayerController player;

		public override void OnAttach()
		{
			player = PlayerController.Instance;
			this.AddObserver(OnTweakSummonAttackCD, AttackingSummon.TweakAttackCDNotification);
		}

		public override void OnUnattach()
		{
			this.RemoveObserver(OnTweakSummonAttackCD, AttackingSummon.TweakAttackCDNotification);
		}

		private void OnTweakSummonAttackCD(object sender, object args)
		{
			if (!((sender as AttackingSummon).SummonTypeID != summonTypeID))
			{
				float toMultiply = player.stats[StatType.FireRate].ModifyInverse(1f);
				(args as List<ValueModifier>).Add(new MultValueModifier(0, toMultiply));
			}
		}
	}
}
