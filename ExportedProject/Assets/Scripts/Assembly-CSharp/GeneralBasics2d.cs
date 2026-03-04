using System;
using UnityEngine;

public class GeneralBasics2d : MonoBehaviour
{
	public Texture2D dudeTexture;

	public GameObject prefabParticles;

	private void Start()
	{
		GameObject gameObject = createSpriteDude("avatarRotate", new Vector3(-2.51208f, 10.7119f, -14.37754f));
		GameObject obj = createSpriteDude("avatarScale", new Vector3(2.51208f, 10.2119f, -14.37754f));
		GameObject gameObject2 = createSpriteDude("avatarMove", new Vector3(-3.1208f, 7.100643f, -14.37754f));
		LeanTween.rotateAround(gameObject, Vector3.forward, -360f, 5f);
		LeanTween.scale(obj, new Vector3(1.7f, 1.7f, 1.7f), 5f).setEase(LeanTweenType.easeOutBounce);
		LeanTween.moveX(obj, obj.transform.position.x + 1f, 5f).setEase(LeanTweenType.easeOutBounce);
		LeanTween.move(gameObject2, gameObject2.transform.position + new Vector3(1.7f, 0f, 0f), 2f).setEase(LeanTweenType.easeInQuad);
		LeanTween.move(gameObject2, gameObject2.transform.position + new Vector3(2f, -1f, 0f), 2f).setDelay(3f);
		LeanTween.scale(obj, new Vector3(0.2f, 0.2f, 0.2f), 1f).setDelay(7f).setEase(LeanTweenType.easeInOutCirc)
			.setLoopPingPong(3);
		LeanTween.delayedCall(base.gameObject, 0.2f, advancedExamples);
	}

	private GameObject createSpriteDude(string name, Vector3 pos, bool hasParticles = true)
	{
		GameObject gameObject = new GameObject(name);
		SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 0.70980394f, 1f);
		spriteRenderer.sprite = Sprite.Create(dudeTexture, new Rect(0f, 0f, 256f, 256f), new Vector2(0.5f, 0f), 256f);
		gameObject.transform.position = pos;
		if (hasParticles)
		{
			GameObject obj = UnityEngine.Object.Instantiate(prefabParticles, Vector3.zero, prefabParticles.transform.rotation);
			obj.transform.parent = gameObject.transform;
			obj.transform.localPosition = prefabParticles.transform.position;
		}
		return gameObject;
	}

	private void advancedExamples()
	{
		LeanTween.delayedCall(base.gameObject, 14f, (Action)delegate
		{
			for (int i = 0; i < 10; i++)
			{
				GameObject rotator = new GameObject("rotator" + i);
				rotator.transform.position = new Vector3(2.71208f, 7.100643f, -12.37754f);
				GameObject obj = createSpriteDude("dude" + i, new Vector3(-2.51208f, 7.100643f, -14.37754f), hasParticles: false);
				obj.transform.parent = rotator.transform;
				obj.transform.localPosition = new Vector3(0f, 0.5f, 0.5f * (float)i);
				obj.transform.localScale = new Vector3(0f, 0f, 0f);
				LeanTween.scale(obj, new Vector3(0.65f, 0.65f, 0.65f), 1f).setDelay((float)i * 0.2f).setEase(LeanTweenType.easeOutBack);
				float num = LeanTween.tau / 10f * (float)i;
				float r = Mathf.Sin(num + LeanTween.tau * 0f / 3f) * 0.5f + 0.5f;
				float g = Mathf.Sin(num + LeanTween.tau * 1f / 3f) * 0.5f + 0.5f;
				float b = Mathf.Sin(num + LeanTween.tau * 2f / 3f) * 0.5f + 0.5f;
				Color to = new Color(r, g, b);
				LeanTween.color(obj, to, 0.3f).setDelay(1.2f + (float)i * 0.4f);
				LeanTween.moveLocalZ(obj, -2f, 0.3f).setDelay(1.2f + (float)i * 0.4f).setEase(LeanTweenType.easeSpring)
					.setOnComplete((Action)delegate
					{
						LeanTween.rotateAround(rotator, Vector3.forward, -1080f, 12f);
					});
				LeanTween.moveLocalY(obj, 1.17f, 1.2f).setDelay(5f + (float)i * 0.2f).setLoopPingPong(1)
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
