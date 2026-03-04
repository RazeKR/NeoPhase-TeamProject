using System.Collections;
using UnityEngine;

namespace flanne.CharacterPassives
{
	public class DumpAmmoPassive : SkillPassive
	{
		public float shotCDMultiplier;

		[SerializeField]
		private Shooter shooter;

		private PlayerController player;

		private Ammo ammo;

		private Gun myGun;

		private bool _isSpraying;

		protected override void Init()
		{
			player = base.transform.root.GetComponent<PlayerController>();
			ammo = player.ammo;
			myGun = player.gun;
			_isSpraying = false;
		}

		protected override void PerformSkill()
		{
			if (!ammo.outOfAmmo && !_isSpraying && player.playerHealth.hp != 0)
			{
				StartCoroutine(SprayCR(ammo.amount));
			}
		}

		private IEnumerator SprayCR(int amountShots)
		{
			_isSpraying = true;
			player.disableAction.Flip();
			myGun.SetVisible(visible: false);
			player.disableAnimation.Flip();
			player.playerAnimator.ResetTrigger("Idle");
			player.playerAnimator.ResetTrigger("Run");
			player.playerAnimator.ResetTrigger("Walk");
			player.playerAnimator.SetTrigger("Special");
			while (ammo.amount > 0)
			{
				ShootRandom();
				yield return new WaitForSeconds(myGun.shotCooldown * shotCDMultiplier);
			}
			player.disableAnimation.UnFlip();
			player.playerAnimator.ResetTrigger("Special");
			player.playerAnimator.SetTrigger("Idle");
			myGun.SetVisible(visible: true);
			_isSpraying = false;
			player.disableAction.UnFlip();
		}

		private void ShootRandom()
		{
			Vector2 vector = Vector2.zero;
			while (vector == Vector2.zero)
			{
				vector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			}
			int count = myGun.shooters.Count;
			for (int i = 0; i < count; i++)
			{
				shooter.Shoot(myGun.GetProjectileRecipe(), vector.Rotate(i * 10), myGun.numOfProjectiles, myGun.spread, 0f);
				myGun.OnShoot.Invoke();
			}
			myGun.gunData.gunshotSFX?.Play();
		}
	}
}
