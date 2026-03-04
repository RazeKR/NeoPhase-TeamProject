using UnityEngine;

public class PathSpline2d : MonoBehaviour
{
	public Transform[] cubes;

	public GameObject dude1;

	public GameObject dude2;

	private LTSpline visualizePath;

	private void Start()
	{
		Vector3[] array = new Vector3[5]
		{
			cubes[0].position,
			cubes[1].position,
			cubes[2].position,
			cubes[3].position,
			cubes[4].position
		};
		visualizePath = new LTSpline(array);
		LeanTween.moveSpline(dude1, array, 10f).setOrientToPath2d(doesOrient2d: true).setSpeed(2f);
		LeanTween.moveSplineLocal(dude2, array, 10f).setOrientToPath2d(doesOrient2d: true).setSpeed(2f);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		if (visualizePath != null)
		{
			visualizePath.gizmoDraw();
		}
	}
}
