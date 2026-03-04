using UnityEngine;

namespace flanne
{
	public class FreezeOnHit : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float chanceToHit;

		public float freezeDuration;

		private FreezeSystem FreezeSys;

		private void Start()
		{
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
			FreezeSys = FreezeSystem.SharedInstance;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			if (Random.Range(0f, 1f) < chanceToHit && (sender as MonoBehaviour).gameObject.tag == "Bullet")
			{
				GameObject gameObject = args as GameObject;
				if (gameObject.tag.Contains("Enemy"))
				{
					FreezeSys.Freeze(gameObject);
				}
			}
		}
	}
}
