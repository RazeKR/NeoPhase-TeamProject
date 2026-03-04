using System;

namespace flanne
{
	[Serializable]
	public class UnlockData
	{
		public bool[] unlocks;

		public UnlockData(int size)
		{
			unlocks = new bool[size];
		}
	}
}
