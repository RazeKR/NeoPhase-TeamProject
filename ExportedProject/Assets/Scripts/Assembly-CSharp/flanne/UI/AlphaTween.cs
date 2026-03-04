using UnityEngine;

namespace flanne.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class AlphaTween : UITweener
	{
		private CanvasGroup canvasGroup;

		private int _tweenID;

		private void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}

		public override void Show()
		{
			LeanTween.cancel(_tweenID);
			_tweenID = LeanTween.alphaCanvas(canvasGroup, 1f, duration).setIgnoreTimeScale(useUnScaledTime: true).id;
		}

		public override void Hide()
		{
			LeanTween.cancel(_tweenID);
			_tweenID = LeanTween.alphaCanvas(canvasGroup, 0f, duration).setIgnoreTimeScale(useUnScaledTime: true).id;
		}

		public override void SetOff()
		{
			canvasGroup.alpha = 0f;
		}
	}
}
