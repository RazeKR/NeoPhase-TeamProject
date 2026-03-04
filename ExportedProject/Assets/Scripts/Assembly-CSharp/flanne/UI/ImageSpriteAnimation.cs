using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class ImageSpriteAnimation : MonoBehaviour
	{
		[SerializeField]
		private Image image;

		[SerializeField]
		private float secPerFrame;

		[SerializeField]
		private Sprite[] sprites;

		[SerializeField]
		private bool isLooping;

		[SerializeField]
		private float delayBetweenLoops;

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
				image.sprite = sprites[i];
				yield return new WaitForSecondsRealtime(secPerFrame);
			}
			while (isLooping)
			{
				yield return new WaitForSecondsRealtime(delayBetweenLoops);
				for (int i = 0; i < sprites.Length; i++)
				{
					image.sprite = sprites[i];
					yield return new WaitForSecondsRealtime(secPerFrame);
				}
			}
		}
	}
}
