using UnityEngine;

public class PathSplineTrack : MonoBehaviour
{
	public GameObject car;

	public GameObject carInternal;

	public GameObject trackTrailRenderers;

	public Transform[] trackOnePoints;

	private LTSpline track;

	private int trackIter = 1;

	private float trackPosition;

	private void Start()
	{
		track = new LTSpline(new Vector3[7]
		{
			trackOnePoints[0].position,
			trackOnePoints[1].position,
			trackOnePoints[2].position,
			trackOnePoints[3].position,
			trackOnePoints[4].position,
			trackOnePoints[5].position,
			trackOnePoints[6].position
		});
		LeanTween.moveSpline(trackTrailRenderers, track, 2f).setOrientToPath(doesOrient: true).setRepeat(-1);
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
		trackPosition += Time.deltaTime * 0.03f;
		if (trackPosition < 0f)
		{
			trackPosition = 1f;
		}
		else if (trackPosition > 1f)
		{
			trackPosition = 0f;
		}
	}

	private void OnDrawGizmos()
	{
		LTSpline.drawGizmo(trackOnePoints, Color.red);
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
