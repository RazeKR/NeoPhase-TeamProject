using UnityEngine;

namespace flanne
{
	public class FlatDamageUp : MonoBehaviour
	{
		public int damage;

		private void Start()
		{
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			if ((sender as MonoBehaviour).gameObject.tag == "Bullet")
			{
				GameObject gameObject = args as GameObject;
				if (gameObject.tag.Contains("Enemy"))
				{
					gameObject.gameObject.GetComponent<Health>()?.HPChange(-1 * damage);
				}
			}
		}
	}
}
