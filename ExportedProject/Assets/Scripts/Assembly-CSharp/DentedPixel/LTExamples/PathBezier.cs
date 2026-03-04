using UnityEngine;

namespace DentedPixel.LTExamples
{
	public class PathBezier : MonoBehaviour
	{
		public Transform[] trans;

		private LTBezierPath cr;

		private GameObject avatar1;

		private float iter;

		private void OnEnable()
		{
			cr = new LTBezierPath(new Vector3[8]
			{
				trans[0].position,
				trans[2].position,
				trans[1].position,
				trans[3].position,
				trans[3].position,
				trans[5].position,
				trans[4].position,
				trans[6].position
			});
		}

		private void Start()
		{
			avatar1 = GameObject.Find("Avatar1");
			LTDescr lTDescr = LeanTween.move(avatar1, cr.pts, 6.5f).setOrientToPath(doesOrient: true).setRepeat(-1);
			Debug.Log("length of path 1:" + cr.length);
			Debug.Log("length of path 2:" + lTDescr.optional.path.length);
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
			if (cr != null)
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
}
