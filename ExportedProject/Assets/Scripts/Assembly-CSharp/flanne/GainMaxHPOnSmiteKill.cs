using UnityEngine;

namespace flanne
{
	public class GainMaxHPOnSmiteKill : MonoBehaviour
	{
		[SerializeField]
		private int killsToGainMHP;

		[SerializeField]
		private int mhpGainCap;

		private PlayerController player;

		private StatsHolder playerStats;

		private PlayerHealth playerHealth;

		private int _killCounter;

		private int _mhpGainCounter;

		private void OnSmiteKill(object sender, object args)
		{
			if (_mhpGainCounter < mhpGainCap)
			{
				_killCounter++;
				if (_killCounter >= killsToGainMHP)
				{
					_killCounter = 0;
					playerStats[StatType.MaxHP].AddFlatBonus(1);
					int a = Mathf.FloorToInt(playerStats[StatType.MaxHP].Modify(player.loadedCharacter.startHP));
					playerHealth.maxHP = Mathf.Min(a, 20);
					_mhpGainCounter++;
				}
			}
		}

		private void Start()
		{
			player = GetComponentInParent<PlayerController>();
			playerStats = player.stats;
			playerHealth = player.playerHealth;
			this.AddObserver(OnSmiteKill, SmitePassive.SmiteKillNotification);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnSmiteKill, SmitePassive.SmiteKillNotification);
		}
	}
}
