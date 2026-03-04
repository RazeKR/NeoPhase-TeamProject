using System;
using UnityEngine;

namespace flanne
{
	public class Projectile : MonoBehaviour
	{
		public static string ImpactEvent = "Projectile.ImpactEvent";

		public static string KillEvent = "Projectile.KillEvent";

		public static string TweakPierceBounce = "Projectile.TweakPierceBounce";

		public static string BounceEvent = "Projectile.BounceEvent";

		[SerializeField]
		protected Rigidbody2D rb;

		[SerializeField]
		protected Knockback kb;

		[SerializeField]
		protected MoveComponent2D move;

		[SerializeField]
		protected TrailRenderer trail;

		public int bounce;

		public int piercing;

		public bool dontRotateOnBounce;

		[NonSerialized]
		public bool isSecondary;

		[NonSerialized]
		public GameObject owner;

		private int _damage;

		public virtual float damage
		{
			get
			{
				return _damage;
			}
			set
			{
				_damage = Mathf.FloorToInt(value);
			}
		}

		public float knockback
		{
			set
			{
				kb.knockbackForce = value;
			}
		}

		public float angle
		{
			set
			{
				rb.rotation = value;
			}
		}

		public float size
		{
			set
			{
				SetSize(Mathf.Clamp(value, 0f, 5f));
			}
		}

		public Vector2 vector
		{
			get
			{
				return move.vector;
			}
			set
			{
				move.vector = value;
			}
		}

		protected virtual void OnCollisionEnter2D(Collision2D other)
		{
			Health component = other.gameObject.GetComponent<Health>();
			if (component == null)
			{
				return;
			}
			component.TakeDamage(DamageType.Bullet, _damage);
			if (component.isDead)
			{
				this.PostNotification(KillEvent);
			}
			owner.PostNotification(ImpactEvent, other.gameObject);
			this.PostNotification(TweakPierceBounce);
			if (piercing == 0)
			{
				if (bounce == 0)
				{
					base.gameObject.SetActive(value: false);
					return;
				}
				bounce--;
				AIComponent component2 = other.gameObject.GetComponent<AIComponent>();
				BounceOffEnemy(component2);
			}
			else
			{
				piercing--;
			}
		}

		protected virtual void SetSize(float size)
		{
			base.transform.localScale = size * Vector3.one;
			if (trail != null)
			{
				trail.widthMultiplier = size;
			}
		}

		protected void BounceOffEnemy(AIComponent enemy)
		{
			this.PostNotification(BounceEvent, enemy.gameObject);
			if (enemy != null)
			{
				Vector2 vector = enemy.transform.position;
				Transform transform = EnemyFinder.GetClosestEnemy(vector, enemy)?.transform;
				if (transform != null)
				{
					move.vector = (new Vector2(transform.position.x, transform.position.y) - vector).normalized * move.vector.magnitude;
					if (!dontRotateOnBounce)
					{
						angle = Mathf.Atan2(move.vector.y, move.vector.x) * 57.29578f;
					}
					return;
				}
			}
			float magnitude = move.vector.magnitude;
			Vector2 v = Vector2.Reflect(base.transform.right, move.vector).normalized * magnitude;
			v = v.Rotate(UnityEngine.Random.Range(-45, 45));
			move.vector = v.normalized * magnitude;
			angle = Mathf.Atan2(move.vector.y, move.vector.x) * 57.29578f;
		}
	}
}
