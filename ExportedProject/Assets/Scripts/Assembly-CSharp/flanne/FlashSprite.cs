using System.Collections;
using UnityEngine;

namespace flanne
{
	public class FlashSprite : MonoBehaviour
	{
		[Tooltip("Defaults to sprite on this gameObject if left null.")]
		[SerializeField]
		private SpriteRenderer sprite;

		[Tooltip("Material to switch to during the flash.")]
		[SerializeField]
		private Material flashMaterial;

		[Tooltip("Duration of a flash.")]
		[SerializeField]
		private float duration;

		[Tooltip("Times to repeat flash.")]
		[SerializeField]
		private int numRepeats = 1;

		private SpriteRenderer spriteRenderer;

		private Material originalMaterial;

		private Coroutine flashRoutine;

		private void Start()
		{
			if (sprite != null)
			{
				spriteRenderer = sprite;
			}
			else
			{
				spriteRenderer = GetComponent<SpriteRenderer>();
			}
			originalMaterial = spriteRenderer.sharedMaterial;
		}

		private void OnDisable()
		{
			StopFlash();
		}

		public void Flash()
		{
			if (flashRoutine != null)
			{
				StopCoroutine(flashRoutine);
			}
			if (base.gameObject.activeSelf)
			{
				flashRoutine = StartCoroutine(FlashRoutine());
			}
		}

		public void StopFlash()
		{
			if (flashRoutine != null)
			{
				StopCoroutine(flashRoutine);
				spriteRenderer.material = originalMaterial;
				flashRoutine = null;
			}
		}

		private IEnumerator FlashRoutine()
		{
			if (spriteRenderer != null)
			{
				for (int i = 0; i < numRepeats; i++)
				{
					spriteRenderer.material = flashMaterial;
					yield return new WaitForSeconds(duration);
					spriteRenderer.material = originalMaterial;
					yield return new WaitForSeconds(duration);
				}
			}
			flashRoutine = null;
		}
	}
}
