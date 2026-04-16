using System;
using System.Collections;
using UnityEngine;

namespace flanne
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        protected Rigidbody2D rb;

        [SerializeField]
        protected MoveComponent2D move;

        [SerializeField]
        protected TrailRenderer trail;

        [SerializeField]
        protected GameObject effect;

        public bool damagable = true;

        public int bounce;

        public int piercing;

        [Header("피격 포그 플래시 (안개 밝기 연출)")]
        [Tooltip("적 피격 시 밝아지는 외곽 반경 (월드 유닛)")]
        [SerializeField] private float _fogImpactOuterRadius      = 4f;
        [Tooltip("내부 완전 밝음 비율 (0~1)")]
        [SerializeField] [Range(0f, 1f)] private float _fogImpactInnerRatio       = 0.3f;
        [Tooltip("플래시 최대 밝기 (0~1)")]
        [SerializeField] [Range(0f, 1f)] private float _fogImpactPeakIntensity    = 1f;
        [Tooltip("페이드 아웃 시간 (초)")]
        [SerializeField] private float _fogImpactFadeOutDuration  = 0.3f;

        public bool dontRotateOnBounce;

        [NonSerialized]
        public bool isSecondary;

        [NonSerialized]
        public GameObject owner;

        private int _damage;

        private int _initialPiercing;
        private int _initialBounce;

        private bool isDamagableProj;

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

        private void Awake()
        {
            _initialPiercing = piercing;
            _initialBounce = bounce;
        }

        private void OnEnable()
        {
            piercing = _initialPiercing;
            bounce = _initialBounce;
            if (isDamagableProj) damagable = true;

            if (rb != null) rb.simulated = true;
            if (move != null) move.enabled = true;

            if (effect != null) effect.SetActive(true);
            if (trail != null)
            {
                trail.enabled = false;
                trail.Clear();

                trail.emitting = false;

                Invoke(nameof(ActivateTrail), 0.01f);
            }
        }

        private void ActivateTrail()
        {
            if (trail != null)
            {
                trail.enabled = true;
                trail.emitting = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 발사자 자신은 무시
            if (owner != null && other.gameObject == owner) return;

            if (!damagable) return;

            // 데미지는 IDamageable에만 적용
            // move.vector : 현재 이동 방향을 hitDir로 전달하여 HitFlash·데미지텍스트 연출 활성화
            IDamageable damageable = other.gameObject.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage, move.vector.normalized);

                // 피격 위치에서 안개 밝기 플래시 연출
                CFogFlashSource.SpawnImpact(
                    transform.position,
                    _fogImpactOuterRadius,
                    _fogImpactInnerRatio,
                    _fogImpactPeakIntensity,
                    _fogImpactFadeOutDuration);
            }

            // 소멸/관통/반사는 IDamageable 여부와 무관하게 처리
            // (적이든 벽이든 pierce=0, bounce=0이면 총알 소멸)
            if (piercing == 0)
            {
                //DetachEffects();
                StartCoroutine(DisableSequence());
                return;
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

        private void DetachEffects()
        {
            if (trail != null)
            {
                trail.transform.SetParent(null);
                trail.emitting = false;
                Destroy(trail.gameObject, trail.time);
            }

            if (effect != null)
            {
                effect.transform.SetParent(null);
                Destroy(effect, 0.5f);
            }
        }


        private IEnumerator DisableSequence()
        {
            if (damagable)
            {
                damagable = false;
                isDamagableProj = true;
            }
            else
            {
                isDamagableProj = false;
            }

            if (rb != null) rb.simulated = false;
            if (move != null) move.enabled = false;

            if (effect != null)
            {
                yield return new WaitForSeconds(0.5f);
                effect.SetActive(false);
            }
            else if (trail != null)
            {
                yield return new WaitForSeconds(trail.time);
                trail.enabled = false;
            }
            else
            {
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
