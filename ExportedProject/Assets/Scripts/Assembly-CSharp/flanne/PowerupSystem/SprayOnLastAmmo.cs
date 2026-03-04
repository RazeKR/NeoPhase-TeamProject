using System.Collections;
using UnityEngine;

namespace flanne.PowerupSystem
{
	public class SprayOnLastAmmo : MonoBehaviour
	{
		[SerializeField]
		private string bulletOPTag;

		[SerializeField]
		private int numOfBullets;

		[SerializeField]
		private float damageMultiplier;

		[SerializeField]
		private float delayBetweenShots;

		private ProjectileFactory PF;

		private PlayerController player;

		private Gun myGun;

		private Ammo ammo;

		private void Start()
		{
			PF = ProjectileFactory.SharedInstance;
			player = GetComponentInParent<PlayerController>();
			myGun = player.gun;
			ammo = player.ammo;
			ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
		}

		private void OnDestroy()
		{
			ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
		}

		private void OnAmmoChanged(int ammoAmount)
		{
			if (ammoAmount == 0)
			{
				StartCoroutine(SprayBulletsCR());
			}
		}

		private IEnumerator SprayBulletsCR()
		{
			Vector2 startDirection = Vector2.zero;
			while (startDirection == Vector2.zero)
			{
				startDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			}
			yield return new WaitForSeconds(myGun.shotCooldown);
			for (int i = 0; i < numOfBullets; i++)
			{
				float degrees = (float)i / (float)numOfBullets * 360f;
				Vector2 direction = startDirection.Rotate(degrees);
				PF.SpawnProjectile(myGun.GetProjectileRecipe(), direction, base.transform.position, damageMultiplier);
				myGun.gunData.gunshotSFX?.Play();
				yield return new WaitForSeconds(delayBetweenShots);
			}
		}
	}
}
