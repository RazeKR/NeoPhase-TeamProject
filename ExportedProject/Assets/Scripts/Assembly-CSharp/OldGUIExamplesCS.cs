using UnityEngine;

public class OldGUIExamplesCS : MonoBehaviour
{
	public Texture2D grumpy;

	public Texture2D beauty;

	private float w;

	private float h;

	private LTRect buttonRect1;

	private LTRect buttonRect2;

	private LTRect buttonRect3;

	private LTRect buttonRect4;

	private LTRect grumpyRect;

	private LTRect beautyTileRect;

	private void Start()
	{
		w = Screen.width;
		h = Screen.height;
		buttonRect1 = new LTRect(0.1f * w, 0.8f * h, 0.2f * w, 0.14f * h);
		buttonRect2 = new LTRect(1.2f * w, 0.8f * h, 0.2f * w, 0.14f * h);
		buttonRect3 = new LTRect(0.35f * w, 0f * h, 0.3f * w, 0.2f * h, 0f);
		buttonRect4 = new LTRect(0f * w, 0.4f * h, 0.3f * w, 0.2f * h, 1f, 15f);
		grumpyRect = new LTRect(0.5f * w - (float)grumpy.width * 0.5f, 0.5f * h - (float)grumpy.height * 0.5f, grumpy.width, grumpy.height);
		beautyTileRect = new LTRect(0f, 0f, 1f, 1f);
		LeanTween.move(buttonRect2, new Vector2(0.55f * w, buttonRect2.rect.y), 0.7f).setEase(LeanTweenType.easeOutQuad);
	}

	public void catMoved()
	{
		Debug.Log("cat moved...");
	}

	private void OnGUI()
	{
		GUI.DrawTexture(grumpyRect.rect, grumpy);
		if (GUI.Button(new Rect(0f * w, 0f * h, 0.2f * w, 0.14f * h), "Move Cat") && !LeanTween.isTweening(grumpyRect))
		{
			Vector2 to = new Vector2(grumpyRect.rect.x, grumpyRect.rect.y);
			LeanTween.move(grumpyRect, new Vector2(1f * (float)Screen.width - (float)grumpy.width, 0f * (float)Screen.height), 1f).setEase(LeanTweenType.easeOutBounce).setOnComplete(catMoved);
			LeanTween.move(grumpyRect, to, 1f).setDelay(1f).setEase(LeanTweenType.easeOutBounce);
		}
		if (GUI.Button(buttonRect1.rect, "Scale Centered"))
		{
			LeanTween.scale(buttonRect1, new Vector2(buttonRect1.rect.width, buttonRect1.rect.height) * 1.2f, 0.25f).setEase(LeanTweenType.easeOutQuad);
			LeanTween.move(buttonRect1, new Vector2(buttonRect1.rect.x - buttonRect1.rect.width * 0.1f, buttonRect1.rect.y - buttonRect1.rect.height * 0.1f), 0.25f).setEase(LeanTweenType.easeOutQuad);
		}
		if (GUI.Button(buttonRect2.rect, "Scale"))
		{
			LeanTween.scale(buttonRect2, new Vector2(buttonRect2.rect.width, buttonRect2.rect.height) * 1.2f, 0.25f).setEase(LeanTweenType.easeOutBounce);
		}
		if (GUI.Button(new Rect(0.76f * w, 0.53f * h, 0.2f * w, 0.14f * h), "Flip Tile"))
		{
			LeanTween.move(beautyTileRect, new Vector2(0f, beautyTileRect.rect.y + 1f), 1f).setEase(LeanTweenType.easeOutBounce);
		}
		GUI.DrawTextureWithTexCoords(new Rect(0.8f * w, 0.5f * h - (float)beauty.height * 0.5f, (float)beauty.width * 0.5f, (float)beauty.height * 0.5f), beauty, beautyTileRect.rect);
		if (GUI.Button(buttonRect3.rect, "Alpha"))
		{
			LeanTween.alpha(buttonRect3, 0f, 1f).setEase(LeanTweenType.easeOutQuad);
			LeanTween.alpha(buttonRect3, 1f, 1f).setDelay(1f).setEase(LeanTweenType.easeInQuad);
			LeanTween.alpha(grumpyRect, 0f, 1f).setEase(LeanTweenType.easeOutQuad);
			LeanTween.alpha(grumpyRect, 1f, 1f).setDelay(1f).setEase(LeanTweenType.easeInQuad);
		}
		GUI.color = new Color(1f, 1f, 1f, 1f);
		if (GUI.Button(buttonRect4.rect, "Rotate"))
		{
			LeanTween.rotate(buttonRect4, 150f, 1f).setEase(LeanTweenType.easeOutElastic);
			LeanTween.rotate(buttonRect4, 0f, 1f).setDelay(1f).setEase(LeanTweenType.easeOutElastic);
		}
		GUI.matrix = Matrix4x4.identity;
	}
}
