using System;

namespace flanne.Player
{
	public abstract class Buff
	{
		[NonSerialized]
		public PlayerBuffs owner;

		public abstract void OnAttach();

		public abstract void OnUnattach();
	}
}
