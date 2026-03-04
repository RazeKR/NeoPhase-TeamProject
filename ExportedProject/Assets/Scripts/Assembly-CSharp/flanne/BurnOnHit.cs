using UnityEngine;

namespace flanne
{
	public class BurnOnHit : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float chanceToHit;

		public int burnDamge;

		private BurnSystem BurnSys;

		private void Start()
		{
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
			BurnSys = BurnSystem.SharedInstance;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			if (Random.Range(0f, 1f) < chanceToHit)
			{
				GameObject gameObject = args as GameObject;
				if (gameObject.tag.Contains("Enemy"))
				{
					BurnSys.Burn(gameObject, burnDamge);
				}
			}
		}
	}
}
