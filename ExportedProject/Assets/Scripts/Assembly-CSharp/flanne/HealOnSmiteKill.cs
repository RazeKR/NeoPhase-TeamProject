using UnityEngine;

namespace flanne
{
	public class HealOnSmiteKill : MonoBehaviour
	{
		[SerializeField]
		private int killsToHeal;

		private PlayerHealth playerHealth;

		private int _killCounter;

		private void OnSmiteKill(object sender, object args)
		{
			_killCounter++;
			if (_killCounter >= killsToHeal)
			{
				_killCounter = 0;
				playerHealth.Heal(1);
			}
		}

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			playerHealth = componentInParent.playerHealth;
			this.AddObserver(OnSmiteKill, SmitePassive.SmiteKillNotification);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnSmiteKill, SmitePassive.SmiteKillNotification);
		}
	}
}
