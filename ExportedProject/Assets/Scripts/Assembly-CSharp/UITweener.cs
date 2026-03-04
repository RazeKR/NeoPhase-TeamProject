using UnityEngine;

public abstract class UITweener : MonoBehaviour
{
	public float duration = 0.1f;

	private void Awake()
	{
		LeanTween.init(50000);
	}

	public abstract void Show();

	public abstract void Hide();

	public abstract void SetOff();
}
