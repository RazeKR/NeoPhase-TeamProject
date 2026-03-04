using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace flanne.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class Panel : MonoBehaviour
	{
		[SerializeField]
		private Selectable gamepadDefaultSelectable;

		private CanvasGroup canvasGroup;

		public bool interactable
		{
			get
			{
				return canvasGroup.interactable;
			}
			set
			{
				canvasGroup.interactable = value;
				canvasGroup.blocksRaycasts = value;
			}
		}

		protected virtual void Start()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			StartCoroutine(DelayStartCR());
		}

		public virtual void Show()
		{
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			UITweener[] componentsInChildren = GetComponentsInChildren<UITweener>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Show();
			}
			if (Gamepad.current != null)
			{
				gamepadDefaultSelectable?.Select();
			}
		}

		public virtual void Hide()
		{
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			UITweener[] componentsInChildren = GetComponentsInChildren<UITweener>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Hide();
			}
		}

		public void SelectDefault()
		{
			gamepadDefaultSelectable?.Select();
		}

		private IEnumerator DelayStartCR()
		{
			yield return null;
			UITweener[] componentsInChildren = GetComponentsInChildren<UITweener>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetOff();
			}
		}
	}
}
