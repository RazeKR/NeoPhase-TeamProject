using System.Collections;
using UnityEngine;

namespace flanne
{
	public class TemporaryStackingBuff : BuffPlayerStats
	{
		[SerializeField]
		private int duration;

		public void Activate()
		{
			StartCoroutine(BuffCR());
		}

		private IEnumerator BuffCR()
		{
			ApplyBuff();
			yield return new WaitForSeconds(duration);
			RemoveBuff();
		}
	}
}
