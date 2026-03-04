using UnityEngine;
using UnityEngine.UI;

public class GeneralUISpace : MonoBehaviour
{
	public RectTransform mainWindow;

	public RectTransform mainParagraphText;

	public RectTransform mainTitleText;

	public RectTransform mainButton1;

	public RectTransform mainButton2;

	public RectTransform pauseRing1;

	public RectTransform pauseRing2;

	public RectTransform pauseWindow;

	public RectTransform chatWindow;

	public RectTransform chatRect;

	public Sprite[] chatSprites;

	public RectTransform chatBar1;

	public RectTransform chatBar2;

	public Text chatText;

	public RectTransform rawImageRect;

	private void Start()
	{
		mainWindow.localScale = Vector3.zero;
		LeanTween.scale(mainWindow, new Vector3(1f, 1f, 1f), 0.6f).setEase(LeanTweenType.easeOutBack);
		LeanTween.alphaCanvas(mainWindow.GetComponent<CanvasGroup>(), 0f, 1f).setDelay(2f).setLoopPingPong()
			.setRepeat(2);
		mainParagraphText.anchoredPosition3D += new Vector3(0f, -10f, 0f);
		LeanTween.textAlpha(mainParagraphText, 0f, 0.6f).setFrom(0f).setDelay(0f);
		LeanTween.textAlpha(mainParagraphText, 1f, 0.6f).setEase(LeanTweenType.easeOutQuad).setDelay(0.6f);
		LeanTween.move(mainParagraphText, mainParagraphText.anchoredPosition3D + new Vector3(0f, 10f, 0f), 0.6f).setEase(LeanTweenType.easeOutQuad).setDelay(0.6f);
		LeanTween.textColor(mainTitleText, new Color(0.52156866f, 29f / 51f, 0.8745098f), 0.6f).setEase(LeanTweenType.easeOutQuad).setDelay(0.6f)
			.setLoopPingPong()
			.setRepeat(-1);
		LeanTween.textAlpha(mainButton2, 1f, 2f).setFrom(0f).setDelay(0f)
			.setEase(LeanTweenType.easeOutQuad);
		LeanTween.alpha(mainButton2, 1f, 2f).setFrom(0f).setDelay(0f)
			.setEase(LeanTweenType.easeOutQuad);
		LeanTween.size(mainButton1, mainButton1.sizeDelta * 1.1f, 0.5f).setDelay(3f).setEaseInOutCirc()
			.setRepeat(6)
			.setLoopPingPong();
		pauseWindow.anchoredPosition3D += new Vector3(0f, 200f, 0f);
		LeanTween.moveY(pauseWindow, pauseWindow.anchoredPosition3D.y + -200f, 0.6f).setEase(LeanTweenType.easeOutSine).setDelay(0.6f);
		RectTransform component = pauseWindow.Find("PauseText").GetComponent<RectTransform>();
		LeanTween.moveZ(component, component.anchoredPosition3D.z - 80f, 1.5f).setEase(LeanTweenType.punch).setDelay(2f);
		LeanTween.rotateAroundLocal(pauseRing1, Vector3.forward, 360f, 12f).setRepeat(-1);
		LeanTween.rotateAroundLocal(pauseRing2, Vector3.forward, -360f, 22f).setRepeat(-1);
		chatWindow.RotateAround(chatWindow.position, Vector3.up, -180f);
		LeanTween.rotateAround(chatWindow, Vector3.up, 180f, 2f).setEase(LeanTweenType.easeOutElastic).setDelay(1.2f);
		LeanTween.play(chatRect, chatSprites).setLoopPingPong();
		LeanTween.color(chatBar2, new Color(0.972549f, 0.2627451f, 36f / 85f, 0.5f), 1.2f).setEase(LeanTweenType.easeInQuad).setLoopPingPong()
			.setDelay(1.2f);
		LeanTween.scale(chatBar2, new Vector2(1f, 0.7f), 1.2f).setEase(LeanTweenType.easeInQuad).setLoopPingPong();
		string origText = chatText.text;
		chatText.text = "";
		LeanTween.value(base.gameObject, 0f, origText.Length, 6f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(delegate(float val)
		{
			chatText.text = origText.Substring(0, Mathf.RoundToInt(val));
		})
			.setLoopClamp()
			.setDelay(2f);
		LeanTween.alpha(rawImageRect, 0f, 1f).setLoopPingPong();
	}
}
