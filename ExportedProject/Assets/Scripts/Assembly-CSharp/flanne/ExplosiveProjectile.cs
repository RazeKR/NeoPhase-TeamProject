using UnityEngine;
using UnityEngine.SceneManagement;

namespace flanne
{
	public class ExplosiveProjectile : Projectile
	{
		public static string ProjExplodeEvent = "ExplosiveProjectile.ProjExplodeEvent";

		[SerializeField]
		private GameObject explosionPrefab;

		[SerializeField]
		private ExplosionShake2D cameraShaker;

		[SerializeField]
		private SoundEffectSO soundFX;

		private bool _initialized;

		private bool _isQuitting;

		private Vector3 _explosionSize;

		private ObjectPooler OP;

		private int contactDamage => Mathf.FloorToInt(damage / 2f);

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(explosionPrefab.name, explosionPrefab, 50);
			_initialized = true;
			_isQuitting = false;
			SceneManager.activeSceneChanged += ChangedActiveScene;
		}

		private void OnDestroy()
		{
			SceneManager.activeSceneChanged -= ChangedActiveScene;
		}

		protected override void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.GetComponent<Health>() == null)
			{
				return;
			}
			if (piercing == 0)
			{
				if (bounce == 0)
				{
					base.gameObject.SetActive(value: false);
					return;
				}
				DealContactDamage(other.gameObject.GetComponent<Health>(), other);
				bounce--;
				AIComponent component = other.gameObject.GetComponent<AIComponent>();
				BounceOffEnemy(component);
			}
			else
			{
				DealContactDamage(other.gameObject.GetComponent<Health>(), other);
				piercing--;
			}
		}

		private void OnDisable()
		{
			if (_initialized && !_isQuitting)
			{
				SpawnExplosive();
			}
		}

		private void OnApplicationQuit()
		{
			_isQuitting = true;
		}

		private void ChangedActiveScene(Scene current, Scene next)
		{
			_isQuitting = true;
		}

		public void SpawnExplosive()
		{
			GameObject pooledObject = OP.GetPooledObject(explosionPrefab.name);
			pooledObject.GetComponent<Harmful>().damageAmount = Mathf.FloorToInt(damage);
			pooledObject.transform.position = base.transform.position;
			pooledObject.transform.localScale = _explosionSize;
			pooledObject.SetActive(value: true);
			cameraShaker?.Shake();
			soundFX?.Play();
			if (!isSecondary)
			{
				this.PostNotification(ProjExplodeEvent);
			}
		}

		protected override void SetSize(float size)
		{
			_explosionSize = new Vector3(size, size, 1f);
			base.SetSize(size);
		}

		private void DealContactDamage(Health health, Collision2D other)
		{
			if (!(health == null))
			{
				health.HPChange(-1 * contactDamage);
			}
		}
	}
}
