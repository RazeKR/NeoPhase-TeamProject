using System.Collections;
using UnityEngine;

namespace flanne
{
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager Instance;

		[SerializeField]
		private AudioSource musicSource;

		[SerializeField]
		private AudioLowPassFilter musicLowPassFilter;

		private float _musicVolume;

		private float _sfxVolume;

		private IEnumerator _musicFadeCR;

		public float MusicVolume
		{
			get
			{
				return _musicVolume;
			}
			set
			{
				_musicVolume = value;
				musicSource.volume = _musicVolume;
				PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
			}
		}

		public float SFXVolume
		{
			get
			{
				return _sfxVolume;
			}
			set
			{
				_sfxVolume = value;
				PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
			}
		}

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			Object.DontDestroyOnLoad(base.gameObject);
			MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
			SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
		}

		public void FadeInMusic(float fadeDuration)
		{
			StopMusicFade();
			_musicFadeCR = FadeInMusicCR(fadeDuration);
			StartCoroutine(_musicFadeCR);
		}

		public void FadeOutMusic(float fadeDuration)
		{
			StopMusicFade();
			_musicFadeCR = FadeOutMusicCR(fadeDuration);
			StartCoroutine(_musicFadeCR);
		}

		public void PlayMusic(AudioClip clip)
		{
			musicSource.clip = clip;
			musicSource.Play();
		}

		public void SetLowPassFilter(bool isOn)
		{
			musicLowPassFilter.enabled = isOn;
		}

		private void StopMusicFade()
		{
			if (_musicFadeCR != null)
			{
				StopCoroutine(_musicFadeCR);
				_musicFadeCR = null;
				musicSource.volume = _musicVolume;
			}
		}

		private IEnumerator FadeInMusicCR(float fadeDuration)
		{
			musicSource.volume = 0f;
			while (musicSource.volume < _musicVolume)
			{
				musicSource.volume += _musicVolume * Time.unscaledDeltaTime / fadeDuration;
				yield return null;
			}
			musicSource.volume = _musicVolume;
			_musicFadeCR = null;
		}

		private IEnumerator FadeOutMusicCR(float fadeDuration)
		{
			while (musicSource.volume > 0f)
			{
				musicSource.volume -= _musicVolume * Time.unscaledDeltaTime / fadeDuration;
				yield return null;
			}
			musicSource.Stop();
			musicSource.volume = _musicVolume;
			_musicFadeCR = null;
		}
	}
}
