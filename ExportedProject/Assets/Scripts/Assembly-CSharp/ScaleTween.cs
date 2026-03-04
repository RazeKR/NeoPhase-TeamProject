using UnityEngine;

public class ScaleTween : UITweener
{
	[SerializeField]
	private LeanTweenType easeType = LeanTweenType.linear;

	[SerializeField]
	private float startScaleX = 1f;

	[SerializeField]
	private float startScaleY = 1f;

	private void Awake()
	{
		base.transform.localScale = new Vector3(startScaleX, startScaleY, 1f);
	}

	public override void Show()
	{
		LeanTween.scaleX(base.gameObject, 1f, duration).setEase(easeType).setIgnoreTimeScale(useUnScaledTime: true);
		LeanTween.scaleY(base.gameObject, 1f, duration).setEase(easeType).setIgnoreTimeScale(useUnScaledTime: true);
	}

	public override void Hide()
	{
		LeanTween.scaleX(base.gameObject, startScaleX, duration).setIgnoreTimeScale(useUnScaledTime: true);
		LeanTween.scaleY(base.gameObject, startScaleY, duration).setIgnoreTimeScale(useUnScaledTime: true);
	}

	public override void SetOff()
	{
		Hide();
	}
}
