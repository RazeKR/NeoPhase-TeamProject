using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class PersistentHarmBasedOnGunDamage : MonoBehaviour
	{
		public float multiplier = 1f;

		public DamageType damageType;

		[SerializeField]
		private float secondsPerTick;

		public StatMod tickRate;

		public bool triggerOnHit;

		private Gun playerGun;

		private HashSet<Health> _targets;

		private float _timer;

		public float finalSecondsPerTick => tickRate.ModifyInverse(secondsPerTick);

		private void Awake()
		{
			_targets = new HashSet<Health>();
			tickRate = new StatMod();
		}

		private void Start()
		{
			playerGun = PlayerController.Instance.gun;
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			Health component = other.gameObject.GetComponent<Health>();
			if (component != null)
			{
				_targets.Add(component);
			}
		}

		private void OnCollisionExit2D(Collision2D other)
		{
			Health component = other.gameObject.GetComponent<Health>();
			if (component != null)
			{
				_targets.Remove(component);
			}
		}

		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer >= finalSecondsPerTick)
			{
				_timer -= finalSecondsPerTick;
				DealDamage();
			}
		}

		private void DealDamage()
		{
			foreach (Health item in new HashSet<Health>(_targets))
			{
				if (item.gameObject.activeSelf)
				{
					item.TakeDamage(damageType, Mathf.FloorToInt(multiplier * playerGun.damage));
					if (triggerOnHit)
					{
						PlayerController.Instance.gameObject.PostNotification(Projectile.ImpactEvent, item.gameObject);
					}
				}
			}
		}
	}
}
