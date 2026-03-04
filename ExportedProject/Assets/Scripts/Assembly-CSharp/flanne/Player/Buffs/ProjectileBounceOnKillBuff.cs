namespace flanne.Player.Buffs
{
	public class ProjectileBounceOnKillBuff : Buff
	{
		public override void OnAttach()
		{
			this.AddObserver(OnKill, Projectile.KillEvent);
		}

		public override void OnUnattach()
		{
			this.RemoveObserver(OnKill, Projectile.KillEvent);
		}

		private void OnKill(object sender, object args)
		{
			(sender as Projectile).bounce++;
		}
	}
}
