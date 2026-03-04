using UnityEngine;

namespace flanne
{
	public class BuffDuringHolyShield : MonoBehaviour
	{
		[SerializeField]
		private float reloadRateMulti;

		[SerializeField]
		private float movespeedMulti;

		private StatsHolder stats;

		private PreventDamage holyShield;

		private void OnDamagePrevented()
		{
			Deactivate();
		}

		private void OnCooldownDone()
		{
			Activate();
		}

		private void Start()
		{
			PlayerController componentInParent = base.transform.GetComponentInParent<PlayerController>();
			stats = componentInParent.stats;
			holyShield = base.transform.root.GetComponentInChildren<PreventDamage>();
			if (holyShield.isActive)
			{
				Activate();
			}
			holyShield.OnDamagePrevented.AddListener(OnDamagePrevented);
			holyShield.OnCooldownDone.AddListener(OnCooldownDone);
		}

		private void OnDestroy()
		{
			holyShield.OnDamagePrevented.RemoveListener(OnDamagePrevented);
			holyShield.OnCooldownDone.RemoveListener(OnCooldownDone);
		}

		private void Activate()
		{
			stats[StatType.ReloadRate].AddMultiplierBonus(reloadRateMulti);
			stats[StatType.MoveSpeed].AddMultiplierBonus(movespeedMulti);
		}

		private void Deactivate()
		{
			stats[StatType.ReloadRate].AddMultiplierBonus(-1f * reloadRateMulti);
			stats[StatType.MoveSpeed].AddMultiplierBonus(-1f * movespeedMulti);
		}
	}
}
