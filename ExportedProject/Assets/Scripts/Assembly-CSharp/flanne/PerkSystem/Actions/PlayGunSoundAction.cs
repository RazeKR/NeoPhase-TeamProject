using System;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class PlayGunSoundAction : Action
	{
		[NonSerialized]
		private Gun myGun;

		public override void Init()
		{
			myGun = PlayerController.Instance.gun;
		}

		public override void Activate(GameObject target)
		{
			myGun.gunData.gunshotSFX?.Play();
		}
	}
}
