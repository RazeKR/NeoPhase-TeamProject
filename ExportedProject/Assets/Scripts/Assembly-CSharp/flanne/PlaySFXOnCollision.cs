using UnityEngine;

namespace flanne
{
	public class PlaySFXOnCollision : MonoBehaviour
	{
		[SerializeField]
		private string hitTag;

		[SerializeField]
		private SoundEffectSO soundFX;

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(hitTag))
			{
				soundFX.Play();
			}
		}
	}
}
