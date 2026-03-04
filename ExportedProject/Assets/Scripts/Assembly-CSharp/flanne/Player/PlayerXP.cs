using UnityEngine;

namespace flanne.Player
{
	public class PlayerXP : MonoBehaviour
	{
		public UnityFloatEvent OnXPChanged;

		public UnityFloatEvent OnXPToLevelChanged;

		public UnityIntEvent OnLevelChanged;

		public StatMod xpMultiplier;

		private float xp;

		public int level { get; private set; }

		private float xpToLevel
		{
			get
			{
				float num = level + 1;
				if (num < 20f)
				{
					return num * 10f - 5f;
				}
				if (num < 40f)
				{
					return num * 13f - 6f;
				}
				if (num < 60f)
				{
					return num * 16f - 8f;
				}
				return num * num;
			}
		}

		private void Awake()
		{
			xp = 0f;
			level = 1;
			OnXPToLevelChanged.Invoke(xpToLevel);
			xpMultiplier = new StatMod();
		}

		public void GainXP(float amount)
		{
			xp += xpMultiplier.Modify(amount);
			if (xp > xpToLevel)
			{
				xp -= xpToLevel;
				level++;
				OnLevelChanged.Invoke(level);
				OnXPToLevelChanged.Invoke(xpToLevel);
			}
			OnXPChanged.Invoke(xp);
		}
	}
}
