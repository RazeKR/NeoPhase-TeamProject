using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "NewSoundEffect", menuName = "Audio/New Sound Effect")]
	public class SoundEffectSO : ScriptableObject
	{
		private enum SoundClipPlayOrder
		{
			random = 0,
			in_order = 1,
			reverse = 2
		}

		private static Dictionary<string, int> SoundsPlaying = new Dictionary<string, int>();

		private static readonly float SEMITONES_TO_PITCH_CONVERSION_UNIT = 1.05946f;

		public AudioClip[] clips;

		public Vector2 volume = new Vector2(0.5f, 0.5f);

		public bool useSemitones;

		public Vector2Int semitones = new Vector2Int(0, 0);

		public Vector2 pitch = new Vector2(1f, 1f);

		[SerializeField]
		private SoundClipPlayOrder playOrder;

		[SerializeField]
		private int playIndex;

		public void SyncPitchAndSemitones()
		{
			if (useSemitones)
			{
				pitch.x = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, semitones.x);
				pitch.y = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, semitones.y);
			}
			else
			{
				semitones.x = Mathf.RoundToInt(Mathf.Log10(pitch.x) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
				semitones.y = Mathf.RoundToInt(Mathf.Log10(pitch.y) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
			}
		}

		private AudioClip GetAudioClip()
		{
			AudioClip result = clips[(playIndex < clips.Length) ? playIndex : 0];
			switch (playOrder)
			{
			case SoundClipPlayOrder.in_order:
				playIndex = (playIndex + 1) % clips.Length;
				break;
			case SoundClipPlayOrder.random:
				playIndex = UnityEngine.Random.Range(0, clips.Length);
				break;
			case SoundClipPlayOrder.reverse:
				playIndex = (playIndex + clips.Length - 1) % clips.Length;
				break;
			}
			return result;
		}

		public AudioSource Play(AudioSource audioSourceParam = null)
		{
			try
			{
				SoundsPlaying.Add(base.name, 1);
			}
			catch (ArgumentException)
			{
				if (SoundsPlaying[base.name] >= 10)
				{
					return null;
				}
				SoundsPlaying[base.name]++;
			}
			if (clips.Length == 0)
			{
				Debug.LogError("Missing sound clips for " + base.name);
				return null;
			}
			AudioSource audioSource = audioSourceParam;
			if (audioSourceParam == null)
			{
				audioSource = new GameObject("Sound", typeof(AudioSource)).GetComponent<AudioSource>();
			}
			audioSource.clip = GetAudioClip();
			AudioManager instance = AudioManager.Instance;
			if (instance != null)
			{
				audioSource.volume = instance.SFXVolume * UnityEngine.Random.Range(volume.x, volume.y);
			}
			else
			{
				audioSource.volume = UnityEngine.Random.Range(volume.x, volume.y);
			}
			audioSource.pitch = (useSemitones ? Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, UnityEngine.Random.Range(semitones.x, semitones.y)) : UnityEngine.Random.Range(pitch.x, pitch.y));
			audioSource.Play();
			UnityEngine.Object.Destroy(audioSource.gameObject, audioSource.clip.length / audioSource.pitch);
			RemoveDictionaryEntry(audioSource.clip.length / audioSource.pitch, base.name);
			return audioSource;
		}

		private async void RemoveDictionaryEntry(float delay, string name)
		{
			await Task.Delay(Mathf.FloorToInt(delay * 1000f)).ConfigureAwait(continueOnCapturedContext: false);
			SoundsPlaying[name]--;
		}
	}
}
