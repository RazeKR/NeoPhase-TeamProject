using UnityEngine;

namespace flanne
{
	public class LanguageSetter : MonoBehaviour
	{
		public static string ChangedEvent = "LanguageSetter.ChangedEvent";

		private void Awake()
		{
			LocalizationSystem.language = (LocalizationSystem.Language)PlayerPrefs.GetInt("Language", 0);
		}

		public void SetEN()
		{
			LocalizationSystem.language = LocalizationSystem.Language.English;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetJP()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Japanese;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetCH()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Chinese;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetBR()
		{
			LocalizationSystem.language = LocalizationSystem.Language.BrazilPortuguese;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetTC()
		{
			LocalizationSystem.language = LocalizationSystem.Language.TChinese;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetFR()
		{
			LocalizationSystem.language = LocalizationSystem.Language.French;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetIT()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Italian;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetGR()
		{
			LocalizationSystem.language = LocalizationSystem.Language.German;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetPL()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Polish;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetSP()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Spanish;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetRU()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Russian;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetTR()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Turkish;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetKR()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Korean;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}

		public void SetHU()
		{
			LocalizationSystem.language = LocalizationSystem.Language.Hungarian;
			PlayerPrefs.SetInt("Language", (int)LocalizationSystem.language);
			this.PostNotification(ChangedEvent);
		}
	}
}
