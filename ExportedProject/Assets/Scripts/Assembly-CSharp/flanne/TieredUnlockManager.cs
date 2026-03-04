using System;
using UnityEngine;

namespace flanne
{
	public class TieredUnlockManager : MonoBehaviour
	{
		[SerializeField]
		[SerializeReference]
		private TieredUnlockable[] unlockables;

		public TieredUnlockData unlockData
		{
			get
			{
				TieredUnlockData tieredUnlockData = new TieredUnlockData(unlockables.Length);
				for (int i = 0; i < unlockables.Length; i++)
				{
					tieredUnlockData.unlocks[i] = unlockables[i].level;
				}
				return tieredUnlockData;
			}
		}

		public void LoadData(TieredUnlockData data)
		{
			if (data == null)
			{
				data = new TieredUnlockData(unlockables.Length);
			}
			if (data.unlocks == null)
			{
				data.unlocks = new int[unlockables.Length];
			}
			if (data.unlocks.Length != unlockables.Length)
			{
				Array.Resize(ref data.unlocks, unlockables.Length);
			}
			for (int i = 0; i < unlockables.Length; i++)
			{
				unlockables[i].level = data.unlocks[i];
			}
		}
	}
}
