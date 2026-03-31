using UnityEngine;

namespace flanne
{
    public class TimeToLive : MonoBehaviour
    {
        [SerializeField]
        private float lifetime;

        [SerializeField]
        private bool willDestroy;

        public void Refresh()
        {
            CancelInvoke();
            Invoke("Deactivate", lifetime);
        }

        private void OnEnable()
        {
            Invoke("Deactivate", lifetime);
        }

        private void Deactivate()
        {
            if (willDestroy)
            {
                Object.Destroy(base.gameObject);
            }
            else
            {
                base.gameObject.SetActive(value: false);
            }
        }

        private void OnDisable()
        {
            CancelInvoke();
        }
    }
}
