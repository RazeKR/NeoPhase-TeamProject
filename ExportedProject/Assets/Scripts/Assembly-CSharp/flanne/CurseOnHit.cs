using UnityEngine;

namespace flanne
{
	public class CurseOnHit : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float chanceToHit;

		private CurseSystem CurseSys;

		private void Start()
		{
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
			CurseSys = CurseSystem.Instance;
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
					CurseSys.Curse(gameObject);
				}
			}
		}
	}
}
