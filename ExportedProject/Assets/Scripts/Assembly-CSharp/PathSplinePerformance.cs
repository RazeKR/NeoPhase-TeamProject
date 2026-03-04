using System;
using System.Collections.Generic;
using UnityEngine;

public class PathSplinePerformance : MonoBehaviour
{
	public GameObject trackTrailRenderers;

	public GameObject car;

	public GameObject carInternal;

	public float circleLength = 10f;

	public float randomRange = 1f;

	public int trackNodes = 30;

	public float carSpeed = 30f;

	public float tracerSpeed = 2f;

	private LTSpline track;

	private int trackIter = 1;

	private float carAdd;

	private float trackPosition;

	private void Start()
	{
		Application.targetFrameRate = 240;
		List<Vector3> list = new List<Vector3>();
		float num = 0f;
		int num2 = trackNodes + 1;
		for (int i = 0; i < num2; i++)
		{
			float x = Mathf.Cos(num * ((float)Math.PI / 180f)) * circleLength + UnityEngine.Random.Range(0f, randomRange);
			float z = Mathf.Sin(num * ((float)Math.PI / 180f)) * circleLength + UnityEngine.Random.Range(0f, randomRange);
			list.Add(new Vector3(x, 1f, z));
			num += 360f / (float)trackNodes;
		}
		list[0] = list[list.Count - 1];
		list.Add(list[1]);
		list.Add(list[2]);
		track = new LTSpline(list.ToArray());
		carAdd = carSpeed / track.distance;
		tracerSpeed = track.distance / (carSpeed * 1.2f);
		LeanTween.moveSpline(trackTrailRenderers, track, tracerSpeed).setOrientToPath(doesOrient: true).setRepeat(-1);
	}

	private void Update()
	{
		float axis = Input.GetAxis("Horizontal");
		if (Input.anyKeyDown)
		{
			if (axis < 0f && trackIter > 0)
			{
				trackIter--;
				playSwish();
			}
			else if (axis > 0f && trackIter < 2)
			{
				trackIter++;
				playSwish();
			}
			LeanTween.moveLocalX(carInternal, (float)(trackIter - 1) * 6f, 0.3f).setEase(LeanTweenType.easeOutBack);
		}
		track.place(car.transform, trackPosition);
		trackPosition += Time.deltaTime * carAdd;
		if (trackPosition > 1f)
		{
			trackPosition = 0f;
		}
	}

	private void OnDrawGizmos()
	{
		if (track != null)
		{
			track.drawGizmo(Color.red);
		}
	}

	private void playSwish()
	{
		AnimationCurve volume = new AnimationCurve(new Keyframe(0f, 0.005464481f, 1.83897f, 0f), new Keyframe(0.1114856f, 2.281785f, 0f, 0f), new Keyframe(0.2482903f, 2.271654f, 0f, 0f), new Keyframe(0.3f, 0.01670286f, 0f, 0f));
		AnimationCurve frequency = new AnimationCurve(new Keyframe(0f, 0.00136725f, 0f, 0f), new Keyframe(0.1482391f, 0.005405405f, 0f, 0f), new Keyframe(0.2650336f, 0.002480127f, 0f, 0f));
		LeanAudio.play(LeanAudio.createAudio(volume, frequency, LeanAudio.options().setVibrato(new Vector3[1]
		{
			new Vector3(0.2f, 0.5f, 0f)
		}).setWaveNoise()
			.setWaveNoiseScale(1000f)));
	}
}
