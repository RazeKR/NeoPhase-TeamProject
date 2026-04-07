using UnityEngine;

namespace flanne
{
    public class TimeToLive : MonoBehaviour
    {
        [SerializeField]
        private float lifetime;
        [SerializeField]
        private bool explosable;
        [SerializeField]
        private CProjectileExplosion explosionPrefab;        

        [SerializeField]
        private bool willDestroy;

        /// <summary>
        /// CWeaponDataSO.LifeTime 등 외부에서 수명을 주입할 때 사용합니다.
        /// </summary>
        public void SetLifetime(float newLifetime)
        {
            lifetime = newLifetime;
            CancelInvoke();
            Invoke(nameof(Deactivate), lifetime);
        }

        public void Refresh()
        {
            CancelInvoke();
            Invoke(nameof(Deactivate), lifetime);
        }

        private void OnEnable()
        {
            Invoke(nameof(Deactivate), lifetime);
        }

        private void Deactivate()
        {
            if (explosable && explosionPrefab != null)
            {
                var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                float damage = GetComponent<flanne.Projectile>().damage;
                float scale = transform.localScale.x;
                explosion.Init(damage, gameObject, scale);
            }

            if (willDestroy)
            {
                Object.Destroy(base.gameObject);
            }
            else
            {
                base.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            CancelInvoke();
        }
    }
}
