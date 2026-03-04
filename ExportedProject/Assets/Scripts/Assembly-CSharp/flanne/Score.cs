namespace flanne
{
	public class Score
	{
		public string timeSurvivedString;

		public int timeSurvivedScore;

		public int enemiesKilled;

		public int enemiesKilledScore;

		public int levelsEarned;

		public int levelsEarnedScore;

		public int totalScore => timeSurvivedScore + enemiesKilledScore + levelsEarnedScore;
	}
}
