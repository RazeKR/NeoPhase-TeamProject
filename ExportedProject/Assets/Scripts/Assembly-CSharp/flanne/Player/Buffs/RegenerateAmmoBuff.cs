using System.Collections;
using UnityEngine;

namespace flanne.Player.Buffs
{
	public class RegenerateAmmoBuff : Buff
	{
		[SerializeField]
		private float timePerRegen;

		[SerializeField]
		private int ammoPerRegen;

		private Gun gun;

		private Ammo ammo;

		private IEnumerator _regenCoroutine;

		public override void OnAttach()
		{
			gun = PlayerController.Instance.gun;
			ammo = PlayerController.Instance.ammo;
			_regenCoroutine = RegenAmmoCR();
			ammo.StartCoroutine(_regenCoroutine);
		}

		public override void OnUnattach()
		{
			ammo.StopCoroutine(_regenCoroutine);
		}

		private IEnumerator RegenAmmoCR()
		{
			float timer = 0f;
			while (true)
			{
				yield return null;
				if (!gun.isShooting && ammo.amount != 0)
				{
					timer += Time.deltaTime;
				}
				if (timer > timePerRegen)
				{
					timer -= timePerRegen;
					ammo.GainAmmo(ammoPerRegen);
				}
			}
		}
	}
}
