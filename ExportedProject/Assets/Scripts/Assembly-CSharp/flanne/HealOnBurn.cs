using UnityEngine;

namespace flanne
{
	public class HealOnBurn : MonoBehaviour
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToHeal;

		private PlayerHealth playerHealth;

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			playerHealth = componentInParent.playerHealth;
			this.AddObserver(OnInflictBurn, BurnSystem.InflictBurnEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnInflictBurn, BurnSystem.InflictBurnEvent);
		}

		private void OnInflictBurn(object sender, object args)
		{
			if (Random.Range(0f, 1f) < chanceToHeal)
			{
				playerHealth.Heal(1);
			}
		}
	}
}
