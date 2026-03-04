using System.Collections;
using UnityEngine;

namespace flanne
{
	public class SpriteFXRealtime : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		[SerializeField]
		private float secPerFrame;

		[SerializeField]
		private bool loop;

		[SerializeField]
		private Sprite[] sprites;

		private IEnumerator _coroutine;

		private void OnEnable()
		{
			_coroutine = Play();
			StartCoroutine(_coroutine);
		}

		private void OnDisable()
		{
			StopCoroutine(_coroutine);
		}

		private IEnumerator Play()
		{
			for (int i = 0; i < sprites.Length; i++)
			{
				spriteRenderer.sprite = sprites[i];
				yield return new WaitForSecondsRealtime(secPerFrame);
			}
			while (loop)
			{
				for (int i = 0; i < sprites.Length; i++)
				{
					spriteRenderer.sprite = sprites[i];
					yield return new WaitForSecondsRealtime(secPerFrame);
				}
			}
		}
	}
}
