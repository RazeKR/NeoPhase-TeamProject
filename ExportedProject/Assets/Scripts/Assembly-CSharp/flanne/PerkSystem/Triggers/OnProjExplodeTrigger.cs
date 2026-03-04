namespace flanne.PerkSystem.Triggers
{
	public class OnProjExplodeTrigger : Trigger
	{
		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnExplode, ExplosiveProjectile.ProjExplodeEvent);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnExplode, ExplosiveProjectile.ProjExplodeEvent);
		}

		private void OnExplode(object sender, object args)
		{
			ExplosiveProjectile explosiveProjectile = sender as ExplosiveProjectile;
			RaiseTrigger(explosiveProjectile.gameObject);
		}
	}
}
