using System;
using UnityEngine;

namespace flanne
{
	public class UnlockablesManager : MonoBehaviour
	{
		[SerializeField]
		private Unlockable[] unlockables;

		public UnlockData unlockData
		{
			get
			{
				UnlockData unlockData = new UnlockData(unlockables.Length);
				for (int i = 0; i < unlockables.Length; i++)
				{
					unlockData.unlocks[i] = !unlockables[i].IsLocked;
				}
				return unlockData;
			}
		}

		public void LoadData(UnlockData data)
		{
			if (data == null)
			{
				data = new UnlockData(unlockables.Length);
			}
			if (data.unlocks == null)
			{
				data.unlocks = new bool[unlockables.Length];
			}
			if (data.unlocks.Length != unlockables.Length)
			{
				Array.Resize(ref data.unlocks, unlockables.Length);
			}
			for (int i = 0; i < unlockables.Length; i++)
			{
				if (data.unlocks[i])
				{
					unlockables[i].gameObject.SetActive(value: true);
					unlockables[i].Unlock();
				}
				else
				{
					unlockables[i].Lock();
				}
			}
		}
	}
}
