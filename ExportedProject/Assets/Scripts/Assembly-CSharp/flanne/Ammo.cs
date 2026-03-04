using System;
using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class Ammo : MonoBehaviour
	{
		public static string ShouldConsumeAmmoCheck = "Ammo.ShouldConsumeAmmo";

		[SerializeField]
		private Gun gun;

		public UnityEvent OnReload;

		public UnityEvent OnAmmoGained;

		public UnityIntEvent OnAmmoChanged;

		public UnityIntEvent OnMaxAmmoChanged;

		public BoolToggle infiniteAmmo;

		public bool outOfAmmo => amount == 0;

		public bool fullOnAmmo => amount == gun.maxAmmo;

		public int amount { get; private set; }

		public int max => gun.maxAmmo;

		private void Start()
		{
			infiniteAmmo = new BoolToggle(b: false);
			Reload();
			OnAmmoChanged.Invoke(amount);
			OnMaxAmmoChanged.Invoke(gun.maxAmmo);
			gun.stats[StatType.MaxAmmo].ChangedEvent += AmmoModChanged;
		}

		private void OnDestroy()
		{
			gun.stats[StatType.MaxAmmo].ChangedEvent -= AmmoModChanged;
		}

		public void Reload()
		{
			amount = gun.maxAmmo;
			OnAmmoChanged.Invoke(amount);
			OnReload.Invoke();
		}

		public void UseAmmo(int a = 1)
		{
			if (!infiniteAmmo.value)
			{
				BaseException ex = new BaseException(defaultToggle: true);
				this.PostNotification(ShouldConsumeAmmoCheck, ex);
				if (ex.toggle)
				{
					amount -= a;
					amount = Mathf.Clamp(amount, 0, gun.maxAmmo);
					OnAmmoChanged.Invoke(amount);
				}
			}
		}

		public void GainAmmo(int value = 1)
		{
			amount += value;
			amount = Mathf.Clamp(amount, 0, gun.maxAmmo);
			OnAmmoChanged.Invoke(amount);
			OnAmmoGained.Invoke();
		}

		public void AmmoModChanged(object sender, EventArgs e)
		{
			OnMaxAmmoChanged.Invoke(gun.maxAmmo);
		}
	}
}
