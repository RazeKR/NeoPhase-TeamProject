using System.Collections;
using UnityEngine;

namespace flanne
{
	public class LineAnimator : MonoBehaviour
	{
		[SerializeField]
		private LineRenderer lineRenderer;

		[SerializeField]
		private Texture[] textures;

		[SerializeField]
		private float secPerFrame;

		[SerializeField]
		private bool loop;

		private void Start()
		{
			StartCoroutine(PlayCR());
		}

		private IEnumerator PlayCR()
		{
			for (int i = 0; i < textures.Length; i++)
			{
				lineRenderer.material.SetTexture("_MainTex", textures[i]);
				yield return new WaitForSeconds(secPerFrame);
			}
			while (loop)
			{
				for (int i = 0; i < textures.Length; i++)
				{
					lineRenderer.material.SetTexture("_MainTex", textures[i]);
					yield return new WaitForSeconds(secPerFrame);
				}
			}
		}
	}
}
