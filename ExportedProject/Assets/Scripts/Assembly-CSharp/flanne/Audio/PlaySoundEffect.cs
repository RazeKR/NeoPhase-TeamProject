using UnityEngine;

namespace flanne.Audio
{
	public class PlaySoundEffect : MonoBehaviour
	{
		[SerializeField]
		private SoundEffectSO soundFX;

		public void Play()
		{
			soundFX?.Play();
		}
	}
}
