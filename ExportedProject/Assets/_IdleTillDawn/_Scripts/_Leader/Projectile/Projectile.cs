using System;
using UnityEngine;

namespace flanne
{
    public class Projectile : MonoBehaviour
    {
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
                if (kb != null) kb.knockbackForce = value;
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
            // 플레이어 자신은 무시
            if (owner != null && other.gameObject == owner) return;

            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable == null) return;

            damageable.TakeDamage(_damage);

            // 사망 여부 확인
            CEntityBase entity = other.gameObject.GetComponent<CEntityBase>();
            bool isDead = entity != null && entity.CurrentHealth <= 0;

            if (piercing == 0)
            {
                if (bounce == 0)
                {
                    gameObject.SetActive(false);
                    return;
                }

                bounce--;

                // 충돌 법선 기준 단순 반사
                if (other.contactCount > 0)
                {
                    Vector2 normal = other.contacts[0].normal;
                    float magnitude = move.vector.magnitude;
                    Vector2 reflected = Vector2.Reflect(move.vector.normalized, normal) * magnitude;
                    move.vector = reflected;
                    if (!dontRotateOnBounce)
                        angle = Mathf.Atan2(reflected.y, reflected.x) * Mathf.Rad2Deg;
                }
            }
            else
            {
                piercing--;
            }
        }

        protected virtual void SetSize(float size)
        {
            transform.localScale = size * Vector3.one;
            if (trail != null)
            {
                trail.widthMultiplier = size;
            }
        }
    }
}
