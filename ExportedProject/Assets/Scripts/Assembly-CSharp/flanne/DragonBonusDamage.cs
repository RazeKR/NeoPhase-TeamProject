using UnityEngine;

namespace flanne
{
	public class DragonBonusDamage : MonoBehaviour
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float percentOfDragonsDamage;

		private ShootingSummon _dragon;

		private int damage => Mathf.FloorToInt((float)_dragon.baseDamage * percentOfDragonsDamage);

		private void Start()
		{
			ShootingSummon[] componentsInChildren = GetComponentInParent<PlayerController>().GetComponentsInChildren<ShootingSummon>(includeInactive: true);
			foreach (ShootingSummon shootingSummon in componentsInChildren)
			{
				if (shootingSummon.SummonTypeID == "Dragon")
				{
					_dragon = shootingSummon;
				}
			}
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			if (!(_dragon == null) && (sender as MonoBehaviour).gameObject.tag == "Bullet")
			{
				GameObject gameObject = args as GameObject;
				if (gameObject.tag.Contains("Enemy"))
				{
					gameObject.GetComponent<Health>()?.HPChange(-1 * damage);
				}
			}
		}
	}
}
