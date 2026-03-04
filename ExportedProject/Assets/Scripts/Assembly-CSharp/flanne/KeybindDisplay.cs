using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using flanne.InputExtensions;

namespace flanne
{
	public class KeybindDisplay : MonoBehaviour
	{
		[SerializeField]
		private InputActionReference actionRef;

		[SerializeField]
		private string bindingId;

		[SerializeField]
		private InputBinding.DisplayStringOptions displayOptions;

		[SerializeField]
		private TMP_Text bindingTMP;

		[Tooltip("Can leave null if not using icons.")]
		[SerializeField]
		private Image bindingIcon;

		[Tooltip("Can leave null if not using icons.")]
		[SerializeField]
		private GamepadIcons gamepadIcons;

		private string _currentControlScheme;

		private void Update()
		{
			Refresh();
		}

		private void Refresh()
		{
			string text = string.Empty;
			string deviceLayoutName = null;
			string controlPath = null;
			InputAction inputAction = actionRef?.action;
			if (inputAction != null)
			{
				int num = inputAction.bindings.IndexOf((InputBinding x) => x.id.ToString() == bindingId);
				if (num != -1)
				{
					text = inputAction.GetBindingDisplayString(num, out deviceLayoutName, out controlPath, displayOptions);
				}
			}
			Sprite sprite = gamepadIcons?.GetIcon(deviceLayoutName, controlPath);
			if (bindingIcon == null)
			{
				bindingTMP.text = text;
				bindingTMP.gameObject.SetActive(value: true);
			}
			else if (sprite == null)
			{
				bindingTMP.text = text;
				bindingTMP.gameObject.SetActive(value: true);
				bindingIcon.gameObject.SetActive(value: false);
			}
			else if (deviceLayoutName == "Keyboard")
			{
				bindingTMP.text = text;
				bindingIcon.sprite = sprite;
				bindingTMP.gameObject.SetActive(value: true);
				bindingIcon.gameObject.SetActive(value: true);
			}
			else
			{
				bindingIcon.sprite = sprite;
				bindingIcon.gameObject.SetActive(value: true);
				bindingTMP.gameObject.SetActive(value: false);
			}
		}
	}
}
