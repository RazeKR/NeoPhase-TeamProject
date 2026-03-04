using System.Collections;
using UnityEngine;

namespace flanne.PowerupSystem
{
	public class FireRateOnHurt : MonoBehaviour
	{
		[SerializeField]
		private float duration;

		[SerializeField]
		private float fireRateBoost;

		[SerializeField]
		private float reloadRateBoost;

		private StatsHolder stats;

		private PlayerHealth health;

		private IEnumerator statBoostCoroutine;

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			stats = componentInParent.stats;
			health = componentInParent.playerHealth;
			health.onHurt.AddListener(OnHurt);
		}

		private void OnDestroy()
		{
			health.onHurt.RemoveListener(OnHurt);
		}

		private void OnHurt()
		{
			if (statBoostCoroutine != null)
			{
				StopCoroutine(statBoostCoroutine);
				statBoostCoroutine = null;
				RemoveBoost();
			}
			statBoostCoroutine = StatBoostCR();
			StartCoroutine(statBoostCoroutine);
		}

		private IEnumerator StatBoostCR()
		{
			AddBoost();
			yield return new WaitForSeconds(duration);
			RemoveBoost();
			statBoostCoroutine = null;
		}

		private void AddBoost()
		{
			stats[StatType.FireRate].AddMultiplierBonus(fireRateBoost);
			stats[StatType.ReloadRate].AddMultiplierBonus(fireRateBoost);
		}

		private void RemoveBoost()
		{
			stats[StatType.FireRate].AddMultiplierBonus(-1f * fireRateBoost);
			stats[StatType.ReloadRate].AddMultiplierBonus(-1f * fireRateBoost);
		}
	}
}
