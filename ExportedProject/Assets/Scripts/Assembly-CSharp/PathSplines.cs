using System;
using UnityEngine;

public class PathSplines : MonoBehaviour
{
	public Transform[] trans;

	private LTSpline cr;

	private GameObject avatar1;

	private float iter;

	private void OnEnable()
	{
		cr = new LTSpline(new Vector3[5]
		{
			trans[0].position,
			trans[1].position,
			trans[2].position,
			trans[3].position,
			trans[4].position
		});
	}

	private void Start()
	{
		avatar1 = GameObject.Find("Avatar1");
		LeanTween.move(avatar1, cr, 6.5f).setOrientToPath(doesOrient: true).setRepeat(1)
			.setOnComplete((Action)delegate
			{
				Vector3[] to = new Vector3[5]
				{
					trans[4].position,
					trans[3].position,
					trans[2].position,
					trans[1].position,
					trans[0].position
				};
				LeanTween.moveSpline(avatar1, to, 6.5f);
			})
			.setEase(LeanTweenType.easeOutQuad);
	}

	private void Update()
	{
		iter += Time.deltaTime * 0.07f;
		if (iter > 1f)
		{
			iter = 0f;
		}
	}

	private void OnDrawGizmos()
	{
		if (cr == null)
		{
			OnEnable();
		}
		Gizmos.color = Color.red;
		if (cr != null)
		{
			cr.gizmoDraw();
		}
	}
}
