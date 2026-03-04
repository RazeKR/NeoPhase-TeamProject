using System;
using UnityEngine;

public class GeneralBasic : MonoBehaviour
{
	public GameObject prefabAvatar;

	private void Start()
	{
		GameObject obj = GameObject.Find("AvatarRotate");
		GameObject gameObject = GameObject.Find("AvatarScale");
		GameObject gameObject2 = GameObject.Find("AvatarMove");
		LeanTween.rotateAround(obj, Vector3.forward, 360f, 5f);
		LeanTween.scale(gameObject, new Vector3(1.7f, 1.7f, 1.7f), 5f).setEase(LeanTweenType.easeOutBounce);
		LeanTween.moveX(gameObject, gameObject.transform.position.x + 5f, 5f).setEase(LeanTweenType.easeOutBounce);
		LeanTween.move(gameObject2, gameObject2.transform.position + new Vector3(-9f, 0f, 1f), 2f).setEase(LeanTweenType.easeInQuad);
		LeanTween.move(gameObject2, gameObject2.transform.position + new Vector3(-6f, 0f, 1f), 2f).setDelay(3f);
		LeanTween.scale(gameObject, new Vector3(0.2f, 0.2f, 0.2f), 1f).setDelay(7f).setEase(LeanTweenType.easeInOutCirc)
			.setLoopPingPong(3);
		LeanTween.delayedCall(base.gameObject, 0.2f, advancedExamples);
	}

	private void advancedExamples()
	{
		LeanTween.delayedCall(base.gameObject, 14f, (Action)delegate
		{
			for (int i = 0; i < 10; i++)
			{
				GameObject rotator = new GameObject("rotator" + i);
				rotator.transform.position = new Vector3(10.2f, 2.85f, 0f);
				GameObject obj = UnityEngine.Object.Instantiate(prefabAvatar, Vector3.zero, prefabAvatar.transform.rotation);
				obj.transform.parent = rotator.transform;
				obj.transform.localPosition = new Vector3(0f, 1.5f, 2.5f * (float)i);
				obj.transform.localScale = new Vector3(0f, 0f, 0f);
				LeanTween.scale(obj, new Vector3(0.65f, 0.65f, 0.65f), 1f).setDelay((float)i * 0.2f).setEase(LeanTweenType.easeOutBack);
				float num = LeanTween.tau / 10f * (float)i;
				float r = Mathf.Sin(num + LeanTween.tau * 0f / 3f) * 0.5f + 0.5f;
				float g = Mathf.Sin(num + LeanTween.tau * 1f / 3f) * 0.5f + 0.5f;
				float b = Mathf.Sin(num + LeanTween.tau * 2f / 3f) * 0.5f + 0.5f;
				Color to = new Color(r, g, b);
				LeanTween.color(obj, to, 0.3f).setDelay(1.2f + (float)i * 0.4f);
				LeanTween.moveZ(obj, 0f, 0.3f).setDelay(1.2f + (float)i * 0.4f).setEase(LeanTweenType.easeSpring)
					.setOnComplete((Action)delegate
					{
						LeanTween.rotateAround(rotator, Vector3.forward, -1080f, 12f);
					});
				LeanTween.moveLocalY(obj, 4f, 1.2f).setDelay(5f + (float)i * 0.2f).setLoopPingPong(1)
					.setEase(LeanTweenType.easeInOutQuad);
				LeanTween.alpha(obj, 0f, 0.6f).setDelay(9.2f + (float)i * 0.4f).setDestroyOnComplete(doesDestroy: true)
					.setOnComplete((Action)delegate
					{
						UnityEngine.Object.Destroy(rotator);
					});
			}
		}).setOnCompleteOnStart(isOn: true).setRepeat(-1);
	}
}
