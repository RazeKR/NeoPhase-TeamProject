using CameraShake;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class OptionsSetter : MonoBehaviour
	{
		public static bool AutoReloadEnabled;

		[SerializeField]
		private LocalizedString bgmLabel;

		[SerializeField]
		private LocalizedString sfxLabel;

		[SerializeField]
		private LocalizedString fullscreenLabel;

		[SerializeField]
		private LocalizedString resolutionLabel;

		[SerializeField]
		private LocalizedString cameraShakeLabel;

		[SerializeField]
		private LocalizedString autoReloadLabel;

		[SerializeField]
		private LocalizedString outlineLabel;

		[SerializeField]
		private LocalizedString onString;

		[SerializeField]
		private LocalizedString offString;

		[SerializeField]
		private TMP_Text bgmTMP;

		[SerializeField]
		private TMP_Text sfxTMP;

		[SerializeField]
		private TMP_Text fullscreenTMP;

		[SerializeField]
		private TMP_Text resolutionTMP;

		[SerializeField]
		private TMP_Text cameraShakeTMP;

		[SerializeField]
		private TMP_Text autoReloadTMP;

		[SerializeField]
		private TMP_Text outlineTMP;

		[SerializeField]
		private Button resolutionButton;

		private Vector2Int[] SupportedResolutions = new Vector2Int[4]
		{
			new Vector2Int(800, 450),
			new Vector2Int(1200, 675),
			new Vector2Int(1600, 900),
			new Vector2Int(1920, 1080)
		};

		private int _resolutionIndex;

		private AudioManager AM;

		private void Start()
		{
			AM = AudioManager.Instance;
			Refresh();
		}

		public void Refresh()
		{
			SetBGM(AM.MusicVolume);
			SetSFX(AM.SFXVolume);
			_resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
			bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
			SetFullscreen(fullscreen);
			SetCameraShake(PlayerPrefs.GetInt("CameraShake", 1) == 1);
			SetAutoReload(PlayerPrefs.GetInt("AutoReload", 1) == 1);
			SetOutline(PlayerPrefs.GetInt("Outline", 0) == 1);
		}

		public void OnClickBGMVolume()
		{
			float musicVolume = AM.MusicVolume;
			musicVolume += 0.25f;
			if (musicVolume > 1f)
			{
				musicVolume = 0f;
			}
			SetBGM(musicVolume);
		}

		public void OnClickSFXVolume()
		{
			float sFXVolume = AM.SFXVolume;
			sFXVolume += 0.25f;
			if (sFXVolume > 1f)
			{
				sFXVolume = 0f;
			}
			SetSFX(sFXVolume);
		}

		public void OnClickFullscreen()
		{
			SetFullscreen(!Screen.fullScreen);
		}

		public void OnClickResolution()
		{
			_resolutionIndex++;
			if (_resolutionIndex >= SupportedResolutions.Length)
			{
				_resolutionIndex = 0;
			}
			PlayerPrefs.SetInt("ResolutionIndex", _resolutionIndex);
			SetResolution(_resolutionIndex);
		}

		public void OnClickCameraShake()
		{
			SetCameraShake(!CameraShaker.ShakeOn);
		}

		public void OnClickAutoReload()
		{
			SetAutoReload(!AutoReloadEnabled);
		}

		public void OnClickOutline()
		{
			SetOutline(!OutlineSetter.isOn);
		}

		private void SetBGM(float volume)
		{
			bgmTMP.text = string.Format(LocalizationSystem.GetLocalizedValue(bgmLabel.key) + " {0:P0}.", volume);
			AM.MusicVolume = volume;
		}

		private void SetSFX(float volume)
		{
			sfxTMP.text = string.Format(LocalizationSystem.GetLocalizedValue(sfxLabel.key) + " {0:P0}.", volume);
			AM.SFXVolume = volume;
		}

		private void SetFullscreen(bool isFS)
		{
			string text = LocalizationSystem.GetLocalizedValue(fullscreenLabel.key) + " ";
			text = ((!isFS) ? (text + LocalizationSystem.GetLocalizedValue(offString.key)) : (text + LocalizationSystem.GetLocalizedValue(onString.key)));
			fullscreenTMP.text = text;
			if (isFS)
			{
				DisableResolution();
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullscreen: true);
			}
			else
			{
				resolutionButton.interactable = true;
				SetResolution(_resolutionIndex);
			}
			PlayerPrefs.SetInt("Fullscreen", isFS ? 1 : 0);
		}

		private void SetResolution(int resolutionIndex)
		{
			Vector2Int vector2Int = SupportedResolutions[resolutionIndex];
			Screen.SetResolution(vector2Int.x, vector2Int.y, fullscreen: false);
			string text = LocalizationSystem.GetLocalizedValue(resolutionLabel.key) + " ";
			text = text + vector2Int.x + "x" + vector2Int.y;
			resolutionTMP.text = text;
		}

		private void DisableResolution()
		{
			resolutionButton.interactable = false;
			string text = LocalizationSystem.GetLocalizedValue(resolutionLabel.key) + " ";
			text += "-";
			resolutionTMP.text = text;
		}

		private void SetCameraShake(bool isOn)
		{
			string text = LocalizationSystem.GetLocalizedValue(cameraShakeLabel.key) + " ";
			text = ((!isOn) ? (text + LocalizationSystem.GetLocalizedValue(offString.key)) : (text + LocalizationSystem.GetLocalizedValue(onString.key)));
			cameraShakeTMP.text = text;
			CameraShaker.ShakeOn = isOn;
			PlayerPrefs.SetInt("CameraShake", isOn ? 1 : 0);
		}

		private void SetAutoReload(bool isOn)
		{
			string text = LocalizationSystem.GetLocalizedValue(autoReloadLabel.key) + " ";
			text = ((!isOn) ? (text + LocalizationSystem.GetLocalizedValue(offString.key)) : (text + LocalizationSystem.GetLocalizedValue(onString.key)));
			autoReloadTMP.text = text;
			AutoReloadEnabled = isOn;
			PlayerPrefs.SetInt("AutoReload", isOn ? 1 : 0);
		}

		private void SetOutline(bool isOn)
		{
			string text = LocalizationSystem.GetLocalizedValue(outlineLabel.key) + " ";
			text = ((!isOn) ? (text + LocalizationSystem.GetLocalizedValue(offString.key)) : (text + LocalizationSystem.GetLocalizedValue(onString.key)));
			outlineTMP.text = text;
			OutlineSetter.isOn = isOn;
			PlayerPrefs.SetInt("Outline", isOn ? 1 : 0);
		}
	}
}
