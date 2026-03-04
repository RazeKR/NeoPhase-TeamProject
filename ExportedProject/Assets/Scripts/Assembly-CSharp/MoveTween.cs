using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MoveTween : UITweener
{
	public enum Direction
	{
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3
	}

	[SerializeField]
	private LeanTweenType easeType = LeanTweenType.linear;

	public Direction entranceFrom;

	public float moveAmount;

	private Vector3 moveVector;

	private Vector3 startPosition;

	private RectTransform rectTransform;

	private int _tweenID;

	private void Start()
	{
		switch (entranceFrom)
		{
		case Direction.Left:
			moveVector = new Vector3(-1f * moveAmount, 0f, 0f);
			break;
		case Direction.Right:
			moveVector = new Vector3(moveAmount, 0f, 0f);
			break;
		case Direction.Up:
			moveVector = new Vector3(0f, moveAmount, 0f);
			break;
		case Direction.Down:
			moveVector = new Vector3(0f, -1f * moveAmount, 0f);
			break;
		}
		rectTransform = GetComponent<RectTransform>();
		startPosition = rectTransform.anchoredPosition;
	}

	public override void Show()
	{
		LeanTween.cancel(_tweenID);
		_tweenID = LeanTween.move(rectTransform, startPosition, duration).setEase(easeType).setIgnoreTimeScale(useUnScaledTime: true)
			.id;
	}

	public override void Hide()
	{
		LeanTween.cancel(_tweenID);
		_tweenID = LeanTween.move(rectTransform, startPosition + moveVector, duration).setIgnoreTimeScale(useUnScaledTime: true).id;
	}

	public override void SetOff()
	{
		LeanTween.move(rectTransform, startPosition + moveVector, 0f).setIgnoreTimeScale(useUnScaledTime: true);
	}
}
