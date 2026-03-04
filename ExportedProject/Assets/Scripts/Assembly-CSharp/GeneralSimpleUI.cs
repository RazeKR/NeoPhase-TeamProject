using UnityEngine;
using UnityEngine.UI;

public class GeneralSimpleUI : MonoBehaviour
{
	public RectTransform button;

	private void Start()
	{
		Debug.Log("For better examples see the 4.6_Examples folder!");
		if (button == null)
		{
			Debug.LogError("Button not assigned! Create a new button via Hierarchy->Create->UI->Button. Then assign it to the button variable");
			return;
		}
		LeanTween.value(button.gameObject, button.anchoredPosition, new Vector2(200f, 100f), 1f).setOnUpdate(delegate(Vector2 val)
		{
			button.anchoredPosition = val;
		});
		LeanTween.value(base.gameObject, 1f, 0.5f, 1f).setOnUpdate(delegate(float volume)
		{
			Debug.Log("volume:" + volume);
		});
		LeanTween.value(base.gameObject, base.gameObject.transform.position, base.gameObject.transform.position + new Vector3(0f, 1f, 0f), 1f).setOnUpdate(delegate(Vector3 val)
		{
			base.gameObject.transform.position = val;
		});
		LeanTween.value(base.gameObject, Color.red, Color.green, 1f).setOnUpdate(delegate(Color val)
		{
			((Image)button.gameObject.GetComponent(typeof(Image))).color = val;
		});
		LeanTween.move(button, new Vector3(200f, -100f, 0f), 1f).setDelay(1f);
		LeanTween.rotateAround(button, Vector3.forward, 90f, 1f).setDelay(2f);
		LeanTween.scale(button, button.localScale * 2f, 1f).setDelay(3f);
		LeanTween.rotateAround(button, Vector3.forward, -90f, 1f).setDelay(4f).setEase(LeanTweenType.easeInOutElastic);
	}
}
