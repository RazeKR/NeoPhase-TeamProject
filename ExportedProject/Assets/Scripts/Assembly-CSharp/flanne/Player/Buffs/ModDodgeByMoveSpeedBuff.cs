using System.Collections.Generic;

namespace flanne.Player.Buffs
{
	public class ModDodgeByMoveSpeedBuff : Buff
	{
		public override void OnAttach()
		{
			this.AddObserver(OnTweakDodge, DodgeRoller.TweakDodgeNotification);
		}

		public override void OnUnattach()
		{
			this.RemoveObserver(OnTweakDodge, DodgeRoller.TweakDodgeNotification);
		}

		private void OnTweakDodge(object sender, object args)
		{
			float toMultiply = PlayerController.Instance.stats[StatType.MoveSpeed].Modify(1f);
			(args as List<ValueModifier>).Add(new MultValueModifier(0, toMultiply));
		}
	}
}
