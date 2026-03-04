using System;

namespace flanne
{
	[Serializable]
	public class TieredUnlockData
	{
		public int[] unlocks;

		public TieredUnlockData(int size)
		{
			unlocks = new int[size];
		}
	}
}
