using UnityEngine;

namespace flanne.Player.Buffs
{
	public class ProjectileDamageUpOnBounceBuff : Buff
	{
		[SerializeField]
		private float damageUpPerBounce;

		public override void OnAttach()
		{
			this.AddObserver(OnKill, Projectile.BounceEvent);
		}

		public override void OnUnattach()
		{
			this.RemoveObserver(OnKill, Projectile.BounceEvent);
		}

		private void OnKill(object sender, object args)
		{
			(sender as Projectile).damage *= 1f + damageUpPerBounce;
		}
	}
}
