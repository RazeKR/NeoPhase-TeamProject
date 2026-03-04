using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class ColorTintOnToggle : MonoBehaviour
	{
		[SerializeField]
		private Toggle toggle;

		[SerializeField]
		private Graphic targetGraphic;

		[SerializeField]
		private Color toggleOnColor = Color.white;

		private Color _originalColor;

		private void Awake()
		{
			toggle.onValueChanged.AddListener(delegate
			{
				ToggleValueChanged(toggle);
			});
			_originalColor = targetGraphic.color;
		}

		private void ToggleValueChanged(Toggle change)
		{
			if (toggle.isOn)
			{
				targetGraphic.color = toggleOnColor;
			}
			else
			{
				targetGraphic.color = _originalColor;
			}
		}
	}
}
