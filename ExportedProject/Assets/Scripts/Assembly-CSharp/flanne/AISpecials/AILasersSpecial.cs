using System.Collections;
using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AILaserSpecial", menuName = "AISpecials/AILaserSpecial")]
	public class AILasersSpecial : AISpecial
	{
		[SerializeField]
		private float windupTime;

		[SerializeField]
		private string windupTag;

		[SerializeField]
		private string laserTag;

		[SerializeField]
		private SoundEffectSO laserSFX;

		[SerializeField]
		private SoundEffectSO windupSFX;

		public override void Use(AIComponent ai, Transform target)
		{
			GameObject windupObj = null;
			GameObject laserObj = null;
			for (int i = 0; i < ai.transform.childCount; i++)
			{
				Transform child = ai.transform.GetChild(i);
				if (child.tag == windupTag)
				{
					windupObj = child.gameObject;
				}
				if (child.tag == laserTag)
				{
					laserObj = child.gameObject;
				}
			}
			ai.StartCoroutine(LaserCR(windupObj, laserObj, ai));
		}

		private IEnumerator LaserCR(GameObject windupObj, GameObject laserObj, AIComponent ai)
		{
			windupObj.SetActive(value: true);
			ai.animator?.SetTrigger("Windup");
			windupSFX?.Play();
			yield return new WaitForSeconds(windupTime);
			laserObj.SetActive(value: true);
			ai.animator?.SetTrigger("Special");
			laserSFX?.Play();
		}
	}
}
