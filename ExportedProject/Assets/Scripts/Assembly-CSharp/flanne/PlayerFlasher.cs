using System.Collections;
using UnityEngine;

namespace flanne
{
	public class PlayerFlasher : MonoBehaviour
	{
		[SerializeField]
		private FlashSprite flasher;

		private BoolToggle isFlashing;

		private IEnumerator flashCoroutine;

		private void Start()
		{
			isFlashing = new BoolToggle(b: false);
		}

		public void Flash()
		{
			isFlashing.Flip();
			if (isFlashing.value && flashCoroutine == null)
			{
				flashCoroutine = FlashingCR();
				StartCoroutine(flashCoroutine);
			}
		}

		public void StopFlash()
		{
			isFlashing.UnFlip();
			if (!isFlashing.value && flashCoroutine != null)
			{
				StopCoroutine(flashCoroutine);
				flashCoroutine = null;
			}
		}

		private IEnumerator FlashingCR()
		{
			while (true)
			{
				flasher.Flash();
				yield return new WaitForSeconds(0.5f);
			}
		}
	}
}
