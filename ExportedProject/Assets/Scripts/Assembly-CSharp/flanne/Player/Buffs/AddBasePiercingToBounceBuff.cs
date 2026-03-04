namespace flanne.Player.Buffs
{
	public class AddBasePiercingToBounceBuff : Buff
	{
		public override void OnAttach()
		{
			this.AddObserver(OnTweakPierceBounce, Projectile.TweakPierceBounce);
		}

		public override void OnUnattach()
		{
			this.RemoveObserver(OnTweakPierceBounce, Projectile.TweakPierceBounce);
		}

		private void OnTweakPierceBounce(object sender, object args)
		{
			Projectile obj = sender as Projectile;
			int piercing = obj.piercing;
			obj.piercing = 0;
			obj.bounce += piercing;
		}
	}
}
