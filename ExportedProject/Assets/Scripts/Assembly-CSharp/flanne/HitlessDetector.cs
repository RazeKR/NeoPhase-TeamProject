using UnityEngine;

namespace flanne
{
	public class HitlessDetector : MonoBehaviour
	{
		[SerializeField]
		private PlayerHealth playerHealth;

		public bool hitless { get; private set; }

		private void OnHit()
		{
			hitless = false;
		}

		private void Start()
		{
			hitless = true;
			Invoke("AddListener", 0.01f);
		}

		private void OnDestroy()
		{
			playerHealth.onHurt.RemoveListener(OnHit);
		}

		private void AddListener()
		{
			playerHealth.onHurt.AddListener(OnHit);
		}
	}
}
