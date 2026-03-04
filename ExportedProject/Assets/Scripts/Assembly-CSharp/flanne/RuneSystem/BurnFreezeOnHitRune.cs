using UnityEngine;

namespace flanne.RuneSystem
{
	public class BurnFreezeOnHitRune : Rune
	{
		[Range(0f, 1f)]
		public float chancePerLevel;

		public int burnDamge;

		public float freezeDuration;

		private BurnSystem BurnSys;

		private FreezeSystem FreezeSys;

		protected override void Init()
		{
			PlayerController.Instance.gameObject.AddObserver(OnImpact, Projectile.ImpactEvent);
			BurnSys = BurnSystem.SharedInstance;
			FreezeSys = FreezeSystem.SharedInstance;
		}

		private void OnDestroy()
		{
			PlayerController.Instance.gameObject.RemoveObserver(OnImpact, Projectile.ImpactEvent);
		}

		private void OnImpact(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			if (gameObject.tag.Contains("Enemy"))
			{
				if (Random.Range(0f, 1f) < chancePerLevel * (float)level)
				{
					BurnSys.Burn(gameObject, burnDamge);
				}
				if (Random.Range(0f, 1f) < chancePerLevel * (float)level)
				{
					FreezeSys.Freeze(gameObject);
				}
			}
		}
	}
}
