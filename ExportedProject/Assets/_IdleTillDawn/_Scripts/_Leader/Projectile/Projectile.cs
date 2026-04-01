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
                if (rb != null)
                    rb.rotation = value;
                else
                    transform.rotation = Quaternion.Euler(0f, 0f, value);
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
            // 발사자 자신은 무시
            if (owner != null && other.gameObject == owner) return;

            // 데미지는 IDamageable에만 적용
            // move.vector : 현재 이동 방향을 hitDir로 전달하여 HitFlash·데미지텍스트 연출 활성화
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage, move.vector.normalized);
            }

            // 소멸/관통/반사는 IDamageable 여부와 무관하게 처리
            // (적이든 벽이든 pierce=0, bounce=0이면 총알 소멸)
            if (piercing == 0)
            {
                if (bounce == 0)
                {
                    gameObject.SetActive(false);
                    return;
                }

                // 적에게 맞았을 때만 반사 횟수 차감 (벽은 횟수 유지)
                if (damageable != null)
                {
                    bounce--;
                }

                // 충돌 법선 기준 반사
                if (other.contactCount > 0)
                {
                    Vector2 normal   = other.contacts[0].normal;
                    float   magnitude = move.vector.magnitude;
                    Vector2 reflected = Vector2.Reflect(move.vector.normalized, normal) * magnitude;
                    move.vector = reflected;
                    if (!dontRotateOnBounce)
                        angle = Mathf.Atan2(reflected.y, reflected.x) * Mathf.Rad2Deg;
                }
            }
            else
            {
                // 관통은 적에게 맞았을 때만 차감
                if (damageable != null)
                {
                    piercing--;
                }
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
