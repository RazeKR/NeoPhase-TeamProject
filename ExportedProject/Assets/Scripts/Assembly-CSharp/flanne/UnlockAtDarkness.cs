using TMPro;
using UnityEngine;

namespace flanne
{
	public class UnlockAtDarkness : MonoBehaviour
	{
		[SerializeField]
		private Unlockable unlockable;

		[SerializeField]
		private int darknessReq;

		[SerializeField]
		private TMP_Text unlockConTMP;

		public void CheckUnlock(int diffUnlocked)
		{
			if (diffUnlocked >= darknessReq)
			{
				unlockable.Unlock();
			}
			else
			{
				unlockable.Lock();
			}
			unlockConTMP.enabled = diffUnlocked < darknessReq;
		}
	}
}
