using UnityEngine;

public class PathBezier2d : MonoBehaviour
{
	public Transform[] cubes;

	public GameObject dude1;

	public GameObject dude2;

	private LTBezierPath visualizePath;

	private void Start()
	{
		Vector3[] array = new Vector3[4]
		{
			cubes[0].position,
			cubes[1].position,
			cubes[2].position,
			cubes[3].position
		};
		visualizePath = new LTBezierPath(array);
		LeanTween.move(dude1, array, 10f).setOrientToPath2d(doesOrient2d: true);
		LeanTween.moveLocal(dude2, array, 10f).setOrientToPath2d(doesOrient2d: true);
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
