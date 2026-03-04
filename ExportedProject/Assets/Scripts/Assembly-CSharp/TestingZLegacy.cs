using System;
using System.Collections;
using UnityEngine;

public class TestingZLegacy : MonoBehaviour
{
	public delegate void NextFunc();

	public enum TimingType
	{
		SteadyNormalTime = 0,
		IgnoreTimeScale = 1,
		HalfTimeScale = 2,
		VariableTimeScale = 3,
		Length = 4
	}

	public AnimationCurve customAnimationCurve;

	public Transform pt1;

	public Transform pt2;

	public Transform pt3;

	public Transform pt4;

	public Transform pt5;

	private int exampleIter;

	private string[] exampleFunctions = new string[14]
	{
		"updateValue3Example", "loopTestClamp", "loopTestPingPong", "moveOnACurveExample", "customTweenExample", "moveExample", "rotateExample", "scaleExample", "updateValueExample", "delayedCallExample",
		"alphaExample", "moveLocalExample", "rotateAroundExample", "colorExample"
	};

	public bool useEstimatedTime = true;

	private GameObject ltLogo;

	private TimingType timingType;

	private int descrTimeScaleChangeId;

	private Vector3 origin;

	private void Awake()
	{
	}

	private void Start()
	{
		ltLogo = GameObject.Find("LeanTweenLogo");
		LeanTween.delayedCall(1f, cycleThroughExamples);
		origin = ltLogo.transform.position;
	}

	private void pauseNow()
	{
		Time.timeScale = 0f;
		Debug.Log("pausing");
	}

	private void OnGUI()
	{
		string text = (useEstimatedTime ? "useEstimatedTime" : ("timeScale:" + Time.timeScale));
		GUI.Label(new Rect(0.03f * (float)Screen.width, 0.03f * (float)Screen.height, 0.5f * (float)Screen.width, 0.3f * (float)Screen.height), text);
	}

	private void endlessCallback()
	{
		Debug.Log("endless");
	}

	private void cycleThroughExamples()
	{
		if (exampleIter == 0)
		{
			int num = (int)(timingType + 1);
			if (num > 4)
			{
				num = 0;
			}
			timingType = (TimingType)num;
			useEstimatedTime = timingType == TimingType.IgnoreTimeScale;
			Time.timeScale = (useEstimatedTime ? 0f : 1f);
			if (timingType == TimingType.HalfTimeScale)
			{
				Time.timeScale = 0.5f;
			}
			if (timingType == TimingType.VariableTimeScale)
			{
				descrTimeScaleChangeId = LeanTween.value(base.gameObject, 0.01f, 10f, 3f).setOnUpdate(delegate(float val)
				{
					Time.timeScale = val;
				}).setEase(LeanTweenType.easeInQuad)
					.setUseEstimatedTime(useEstimatedTime: true)
					.setRepeat(-1)
					.id;
			}
			else
			{
				Debug.Log("cancel variable time");
				LeanTween.cancel(descrTimeScaleChangeId);
			}
		}
		base.gameObject.BroadcastMessage(exampleFunctions[exampleIter]);
		float delayTime = 1.1f;
		LeanTween.delayedCall(base.gameObject, delayTime, cycleThroughExamples).setUseEstimatedTime(useEstimatedTime);
		exampleIter = ((exampleIter + 1 < exampleFunctions.Length) ? (exampleIter + 1) : 0);
	}

	public void updateValue3Example()
	{
		Debug.Log("updateValue3Example Time:" + Time.time);
		LeanTween.value(base.gameObject, updateValue3ExampleCallback, new Vector3(0f, 270f, 0f), new Vector3(30f, 270f, 180f), 0.5f).setEase(LeanTweenType.easeInBounce).setRepeat(2)
			.setLoopPingPong()
			.setOnUpdateVector3(updateValue3ExampleUpdate)
			.setUseEstimatedTime(useEstimatedTime);
	}

	public void updateValue3ExampleUpdate(Vector3 val)
	{
	}

	public void updateValue3ExampleCallback(Vector3 val)
	{
		ltLogo.transform.eulerAngles = val;
	}

	public void loopTestClamp()
	{
		Debug.Log("loopTestClamp Time:" + Time.time);
		GameObject obj = GameObject.Find("Cube1");
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		LeanTween.scaleZ(obj, 4f, 1f).setEase(LeanTweenType.easeOutElastic).setRepeat(7)
			.setLoopClamp()
			.setUseEstimatedTime(useEstimatedTime);
	}

	public void loopTestPingPong()
	{
		Debug.Log("loopTestPingPong Time:" + Time.time);
		GameObject obj = GameObject.Find("Cube2");
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		LeanTween.scaleY(obj, 4f, 1f).setEase(LeanTweenType.easeOutQuad).setLoopPingPong(4)
			.setUseEstimatedTime(useEstimatedTime);
	}

	public void colorExample()
	{
		LeanTween.color(GameObject.Find("LCharacter"), new Color(1f, 0f, 0f, 0.5f), 0.5f).setEase(LeanTweenType.easeOutBounce).setRepeat(2)
			.setLoopPingPong()
			.setUseEstimatedTime(useEstimatedTime);
	}

	public void moveOnACurveExample()
	{
		Debug.Log("moveOnACurveExample Time:" + Time.time);
		Vector3[] to = new Vector3[8] { origin, pt1.position, pt2.position, pt3.position, pt3.position, pt4.position, pt5.position, origin };
		LeanTween.move(ltLogo, to, 1f).setEase(LeanTweenType.easeOutQuad).setOrientToPath(doesOrient: true)
			.setUseEstimatedTime(useEstimatedTime);
	}

	public void customTweenExample()
	{
		Debug.Log(string.Concat("customTweenExample starting pos:", ltLogo.transform.position, " origin:", origin));
		LeanTween.moveX(ltLogo, -10f, 0.5f).setEase(customAnimationCurve).setUseEstimatedTime(useEstimatedTime);
		LeanTween.moveX(ltLogo, 0f, 0.5f).setEase(customAnimationCurve).setDelay(0.5f)
			.setUseEstimatedTime(useEstimatedTime);
	}

	public void moveExample()
	{
		Debug.Log("moveExample");
		LeanTween.move(ltLogo, new Vector3(-2f, -1f, 0f), 0.5f).setUseEstimatedTime(useEstimatedTime);
		LeanTween.move(ltLogo, origin, 0.5f).setDelay(0.5f).setUseEstimatedTime(useEstimatedTime);
	}

	public void rotateExample()
	{
		Debug.Log("rotateExample");
		Hashtable hashtable = new Hashtable();
		hashtable.Add("yo", 5.0);
		LeanTween.rotate(ltLogo, new Vector3(0f, 360f, 0f), 1f).setEase(LeanTweenType.easeOutQuad).setOnComplete(rotateFinished)
			.setOnCompleteParam(hashtable)
			.setOnUpdate(rotateOnUpdate)
			.setUseEstimatedTime(useEstimatedTime);
	}

	public void rotateOnUpdate(float val)
	{
	}

	public void rotateFinished(object hash)
	{
		Hashtable hashtable = hash as Hashtable;
		Debug.Log("rotateFinished hash:" + hashtable["yo"]);
	}

	public void scaleExample()
	{
		Debug.Log("scaleExample");
		Vector3 localScale = ltLogo.transform.localScale;
		LeanTween.scale(ltLogo, new Vector3(localScale.x + 0.2f, localScale.y + 0.2f, localScale.z + 0.2f), 1f).setEase(LeanTweenType.easeOutBounce).setUseEstimatedTime(useEstimatedTime);
	}

	public void updateValueExample()
	{
		Debug.Log("updateValueExample");
		Hashtable hashtable = new Hashtable();
		hashtable.Add("message", "hi");
		LeanTween.value(base.gameObject, (Action<float, object>)updateValueExampleCallback, ltLogo.transform.eulerAngles.y, 270f, 1f).setEase(LeanTweenType.easeOutElastic).setOnUpdateParam(hashtable)
			.setUseEstimatedTime(useEstimatedTime);
	}

	public void updateValueExampleCallback(float val, object hash)
	{
		Vector3 eulerAngles = ltLogo.transform.eulerAngles;
		eulerAngles.y = val;
		ltLogo.transform.eulerAngles = eulerAngles;
	}

	public void delayedCallExample()
	{
		Debug.Log("delayedCallExample");
		LeanTween.delayedCall(0.5f, delayedCallExampleCallback).setUseEstimatedTime(useEstimatedTime);
	}

	public void delayedCallExampleCallback()
	{
		Debug.Log("Delayed function was called");
		Vector3 localScale = ltLogo.transform.localScale;
		LeanTween.scale(ltLogo, new Vector3(localScale.x - 0.2f, localScale.y - 0.2f, localScale.z - 0.2f), 0.5f).setEase(LeanTweenType.easeInOutCirc).setUseEstimatedTime(useEstimatedTime);
	}

	public void alphaExample()
	{
		Debug.Log("alphaExample");
		GameObject obj = GameObject.Find("LCharacter");
		LeanTween.alpha(obj, 0f, 0.5f).setUseEstimatedTime(useEstimatedTime);
		LeanTween.alpha(obj, 1f, 0.5f).setDelay(0.5f).setUseEstimatedTime(useEstimatedTime);
	}

	public void moveLocalExample()
	{
		Debug.Log("moveLocalExample");
		GameObject obj = GameObject.Find("LCharacter");
		Vector3 localPosition = obj.transform.localPosition;
		LeanTween.moveLocal(obj, new Vector3(0f, 2f, 0f), 0.5f).setUseEstimatedTime(useEstimatedTime);
		LeanTween.moveLocal(obj, localPosition, 0.5f).setDelay(0.5f).setUseEstimatedTime(useEstimatedTime);
	}

	public void rotateAroundExample()
	{
		Debug.Log("rotateAroundExample");
		LeanTween.rotateAround(GameObject.Find("LCharacter"), Vector3.up, 360f, 1f).setUseEstimatedTime(useEstimatedTime);
	}

	public void loopPause()
	{
		LeanTween.pause(GameObject.Find("Cube1"));
	}

	public void loopResume()
	{
		LeanTween.resume(GameObject.Find("Cube1"));
	}

	public void punchTest()
	{
		LeanTween.moveX(ltLogo, 7f, 1f).setEase(LeanTweenType.punch).setUseEstimatedTime(useEstimatedTime);
	}
}
