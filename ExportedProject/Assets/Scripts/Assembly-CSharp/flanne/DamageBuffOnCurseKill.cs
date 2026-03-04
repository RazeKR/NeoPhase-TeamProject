using UnityEngine;

namespace flanne
{
	public class DamageBuffOnCurseKill : MonoBehaviour
	{
		[SerializeField]
		private float damageBuff;

		[SerializeField]
		private float numKillsPerBuff;

		private StatsHolder playerStats;

		private int _killCounter;

		private void OnCurseKill(object sender, object args)
		{
			_killCounter++;
			if ((float)_killCounter >= numKillsPerBuff)
			{
				_killCounter = 0;
				playerStats[StatType.BulletDamage].AddMultiplierBonus(damageBuff);
			}
		}

		private void Start()
		{
			this.AddObserver(OnCurseKill, CurseSystem.CurseKillNotification);
			playerStats = PlayerController.Instance.stats;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnCurseKill, CurseSystem.CurseKillNotification);
		}
	}
}
