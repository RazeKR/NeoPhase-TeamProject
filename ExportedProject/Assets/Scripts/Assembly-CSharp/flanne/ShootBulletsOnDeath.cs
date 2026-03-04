using System.Collections;
using UnityEngine;

namespace flanne
{
	public class ShootBulletsOnDeath : MonoBehaviour
	{
		private const int maxBullets = 3;

		private static int currentBullets;

		[SerializeField]
		private string bulletOPTag;

		[SerializeField]
		private int numOfBullets;

		[SerializeField]
		private float damageMultiplier;

		private ProjectileFactory PF;

		private Gun myGun;

		private void Start()
		{
			this.AddObserver(OnDeath, Health.DeathEvent);
			PF = ProjectileFactory.SharedInstance;
			myGun = GetComponentInParent<PlayerController>().gun;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDeath(object sender, object args)
		{
			GameObject deathObj = (sender as Health).gameObject;
			if (currentBullets < 3)
			{
				StartCoroutine(ShootOnDeathCR(deathObj));
			}
		}

		private IEnumerator ShootOnDeathCR(GameObject deathObj)
		{
			currentBullets++;
			yield return null;
			if (deathObj.tag == "Enemy")
			{
				Vector2 vector = Vector2.zero;
				while (vector == Vector2.zero)
				{
					vector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
				}
				for (int i = 0; i < numOfBullets; i++)
				{
					float degrees = (float)i / (float)numOfBullets * 360f;
					Vector2 direction = vector.Rotate(degrees);
					ProjectileRecipe projectileRecipe = myGun.GetProjectileRecipe();
					projectileRecipe.size = 0.5f;
					projectileRecipe.knockback *= 0.1f;
					projectileRecipe.projectileSpeed *= 0.35f;
					PF.SpawnProjectile(projectileRecipe, direction, deathObj.transform.position, damageMultiplier, isSecondary: true);
				}
			}
			yield return null;
			yield return null;
			currentBullets--;
		}
	}
}
