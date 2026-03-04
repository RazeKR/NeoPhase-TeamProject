using TMPro;
using UnityEngine;

namespace flanne.UI
{
	[RequireComponent(typeof(TMP_Text))]
	public class TextLocalizerUI : MonoBehaviour
	{
		private TMP_Text tmp;

		[SerializeField]
		private LocalizedString localizedString;

		private void OnLanguageChanged(object sender, object args)
		{
			tmp.text = LocalizationSystem.GetLocalizedValue(localizedString.key);
		}

		private void Start()
		{
			tmp = GetComponent<TMP_Text>();
			tmp.text = LocalizationSystem.GetLocalizedValue(localizedString.key);
			this.AddObserver(OnLanguageChanged, LanguageSetter.ChangedEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnLanguageChanged, LanguageSetter.ChangedEvent);
		}
	}
}
