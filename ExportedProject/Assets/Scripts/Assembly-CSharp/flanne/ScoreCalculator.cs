using UnityEngine;
using flanne.Player;

namespace flanne
{
	public class ScoreCalculator : MonoBehaviour
	{
		public static ScoreCalculator SharedInstance;

		[SerializeField]
		private GameTimer gameTimer;

		[SerializeField]
		private PlayerXP playerXP;

		private int _enemiesKilled;

		private void OnDeath(object sender, object args)
		{
			if ((sender as Health).gameObject.tag == "Enemy")
			{
				_enemiesKilled++;
			}
		}

		private void Awake()
		{
			SharedInstance = this;
		}

		private void Start()
		{
			this.AddObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnDeath, Health.DeathEvent);
		}

		public Score GetScore()
		{
			return new Score
			{
				timeSurvivedString = gameTimer.TimeToString(),
				timeSurvivedScore = Mathf.CeilToInt(gameTimer.timer),
				enemiesKilled = _enemiesKilled,
				enemiesKilledScore = _enemiesKilled,
				levelsEarned = playerXP.level - 1,
				levelsEarnedScore = (playerXP.level - 1) * 100
			};
		}
	}
}
