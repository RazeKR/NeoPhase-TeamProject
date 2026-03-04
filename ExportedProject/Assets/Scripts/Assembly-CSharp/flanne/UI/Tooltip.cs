using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace flanne.UI
{
	public class Tooltip : MonoBehaviour
	{
		public static Tooltip Instance;

		[SerializeField]
		private Camera uiCamera;

		[SerializeField]
		private TMP_Text tooltipText;

		[SerializeField]
		private RectTransform backgroundRectTransform;

		[SerializeField]
		private CanvasGroup cg;

		[SerializeField]
		private Canvas canvas;

		private RectTransform rectTransform;

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
			HideTooltip();
			rectTransform = GetComponent<RectTransform>();
		}

		private void Update()
		{
			Vector2 screenPoint = Mouse.current.position.ReadValue();
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPoint, canvas.worldCamera, out var localPoint);
			rectTransform.position = canvas.transform.TransformPoint(localPoint);
			if (screenPoint.x > (float)(Screen.width / 2))
			{
				backgroundRectTransform.pivot = new Vector2(1f, 0f);
			}
			else
			{
				backgroundRectTransform.pivot = new Vector2(0f, 0f);
			}
		}

		public void ShowTooltip(string tooltipString)
		{
			cg.alpha = 1f;
			tooltipText.text = tooltipString;
			float num = 8f;
			float num2 = 0f;
			num2 = ((!(tooltipText.preferredWidth < ((RectTransform)tooltipText.transform).rect.width)) ? ((RectTransform)tooltipText.transform).rect.width : tooltipText.preferredWidth);
			Vector2 sizeDelta = new Vector2(num2 + num * 2f, tooltipText.preferredHeight + num * 2f);
			backgroundRectTransform.sizeDelta = sizeDelta;
			Update();
		}

		public void HideTooltip()
		{
			cg.alpha = 0f;
		}
	}
}
