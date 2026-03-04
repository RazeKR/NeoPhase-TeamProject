using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using flanne.Core;

namespace flanne
{
	public class Gun : MonoBehaviour
	{
		public static string ShootEvent = "Gun.ShootEvent";

		[SerializeField]
		private PlayerController player;

		[SerializeField]
		private GunData defaultGun;

		[SerializeField]
		private Animator gunEvoAnimator;

		public UnityEvent OnShoot;

		private ShootingCursor SC;

		private IEnumerator _reloadCoroutine;

		private float _shotTimer;

		public StatsHolder stats { get; private set; }

		public GunData gunData { get; private set; }

		public List<Animator> gunAnimators { get; private set; }

		public List<SpriteRenderer> gunSprites { get; private set; }

		public List<Shooter> shooters { get; private set; }

		public GameObject gunObj { get; private set; }

		public float shotCooldown => stats[StatType.FireRate].ModifyInverse(gunData.shotCooldown);

		public float reloadDuration => stats[StatType.ReloadRate].ModifyInverse(gunData.reloadDuration);

		public float damage => stats[StatType.BulletDamage].Modify(gunData.damage);

		public int numOfProjectiles => Mathf.Max(1, (int)stats[StatType.Projectiles].Modify(gunData.numOfProjectiles));

		public float spread => stats[StatType.Spread].Modify(gunData.spread);

		public int maxAmmo => Mathf.Max(1, (int)stats[StatType.MaxAmmo].Modify(gunData.maxAmmo));

		public bool shotReady => _shotTimer <= 0f;

		public bool isShooting { get; private set; }

		private void Start()
		{
			isShooting = false;
			stats = player.stats;
			SC = ShootingCursor.Instance;
		}

		private void Update()
		{
			if (PauseController.isPaused)
			{
				return;
			}
			if (_shotTimer > 0f)
			{
				_shotTimer -= Time.deltaTime;
			}
			else
			{
				if (!isShooting)
				{
					return;
				}
				SetAnimationTrigger("Attack");
				if (gunData.isSummonGun)
				{
					_shotTimer += stats[StatType.SummonAttackSpeed].ModifyInverse(shotCooldown);
				}
				else
				{
					_shotTimer += shotCooldown;
				}
				Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
				Vector2 vector2 = base.transform.position;
				Vector2 pointDirection = vector - vector2;
				foreach (Shooter shooter in shooters)
				{
					ProjectileRecipe projectileRecipe = GetProjectileRecipe();
					this.PostNotification(ShootEvent, projectileRecipe);
					shooter.Shoot(projectileRecipe, pointDirection, numOfProjectiles, spread, gunData.inaccuracy);
				}
				if (!shooters[0].fireOnStop)
				{
					gunData.gunshotSFX?.Play();
				}
			}
		}

		public void LoadGun(GunData gunToLoad)
		{
			if (gunToLoad == null)
			{
				gunData = defaultGun;
			}
			else
			{
				gunData = gunToLoad;
			}
			if (gunObj != null)
			{
				foreach (Shooter shooter in shooters)
				{
					shooter.onShoot.RemoveAllListeners();
				}
				shooters = new List<Shooter>();
				gunAnimators = new List<Animator>();
				gunSprites = new List<SpriteRenderer>();
				Object.Destroy(gunObj);
			}
			gunObj = Object.Instantiate(gunData.model);
			gunObj.transform.SetParent(base.transform);
			gunObj.transform.localPosition = Vector3.zero;
			gunAnimators = gunObj.GetComponentsInChildren<Animator>().ToList();
			if (gunAnimators == null)
			{
				Debug.LogError(string.Concat(gunObj, "is missing an animator."));
			}
			gunSprites = new List<SpriteRenderer>();
			foreach (Animator gunAnimator in gunAnimators)
			{
				gunSprites.AddRange(new List<SpriteRenderer>(gunAnimator.gameObject.GetComponentsInChildren<SpriteRenderer>().ToList()));
			}
			shooters = gunObj.GetComponentsInChildren<Shooter>().ToList();
			if (shooters == null)
			{
				Debug.LogError(string.Concat(gunObj, "is missing an shooter."));
			}
			foreach (Shooter shooter2 in shooters)
			{
				shooter2.onShoot.AddListener(OnShooterShoot);
			}
			ObjectPooler.SharedInstance.AddObject(gunData.bulletOPTag, gunData.bullet, 5000);
		}

		public void AddShooter(Shooter shooter)
		{
			shooters.Add(shooter);
			Animator componentInChildren = shooter.GetComponentInChildren<Animator>();
			gunAnimators.Add(componentInChildren);
		}

		public void SetAnimationTrigger(string trigger)
		{
			foreach (Animator gunAnimator in gunAnimators)
			{
				gunAnimator.SetTrigger(trigger);
			}
		}

		public void SetVisible(bool visible)
		{
			foreach (SpriteRenderer gunSprite in gunSprites)
			{
				gunSprite.enabled = visible;
			}
		}

		public void StartShooting()
		{
			StartCoroutine(WaitToStartShootCR());
		}

		public void StopShooting()
		{
			if (!isShooting)
			{
				return;
			}
			isShooting = false;
			foreach (Shooter shooter in shooters)
			{
				Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
				Vector2 vector2 = base.transform.position;
				shooter.OnStopShoot(pointDirection: vector - vector2, recipe: GetProjectileRecipe(), numProjectiles: numOfProjectiles, spread: spread, inaccuracy: gunData.inaccuracy);
			}
			if (shooters[0].fireOnStop)
			{
				gunData.gunshotSFX?.Play();
			}
		}

		public ProjectileRecipe GetProjectileRecipe()
		{
			ProjectileRecipe projectileRecipe = new ProjectileRecipe();
			projectileRecipe.objectPoolTag = gunData.bulletOPTag;
			if (gunData.isSummonGun)
			{
				projectileRecipe.damage = stats[StatType.SummonDamage].Modify(stats[StatType.BulletDamage].Modify(gunData.damage));
			}
			else
			{
				projectileRecipe.damage = stats[StatType.BulletDamage].Modify(gunData.damage);
			}
			projectileRecipe.projectileSpeed = stats[StatType.ProjectileSpeed].Modify(gunData.projectileSpeed);
			projectileRecipe.size = stats[StatType.ProjectileSize].Modify(1f);
			projectileRecipe.knockback = stats[StatType.Knockback].Modify(gunData.knockback);
			projectileRecipe.bounce = Mathf.Max(0, (int)stats[StatType.Bounce].Modify(gunData.bounce));
			projectileRecipe.piercing = Mathf.Max(0, (int)stats[StatType.Piercing].Modify(gunData.piercing));
			projectileRecipe.owner = player.gameObject;
			return projectileRecipe;
		}

		public void PlayGunEvoAnimation()
		{
			gunEvoAnimator.SetTrigger("Start");
		}

		public void EndGunEvoAnimation()
		{
			gunEvoAnimator.SetTrigger("End");
		}

		private IEnumerator WaitToStartShootCR()
		{
			yield return null;
			isShooting = true;
		}

		private void OnShooterShoot()
		{
			OnShoot.Invoke();
		}
	}
}
