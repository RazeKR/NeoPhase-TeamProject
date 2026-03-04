using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class BurnOnHitBelowAmmoThreshold : MonoBehaviour
	{
		public int burnDamge;

		[Range(0f, 1f)]
		public float percentThreshold;

		public int burnDamageBelowThreshold;

		private BurnSystem BurnSys;

		private Ammo ammo;

		public UnityEvent onActivate;

		public UnityEvent onDeactivate;

		private bool _active;

		private void Start()
		{
			BurnSys = BurnSystem.SharedInstance;
			ammo = PlayerController.Instance.ammo;
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
			ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			if (gameObject.tag.Contains("Enemy"))
			{
				if (_active)
				{
					BurnSys.Burn(gameObject, burnDamageBelowThreshold);
				}
				else
				{
					BurnSys.Burn(gameObject, burnDamge);
				}
			}
		}

		private void OnAmmoChanged(int amount)
		{
			float num = (float)ammo.amount / (float)ammo.max;
			if (_active != num <= percentThreshold)
			{
				_active = num <= percentThreshold;
				if (_active)
				{
					onActivate.Invoke();
				}
				else
				{
					onDeactivate.Invoke();
				}
			}
		}
	}
}
