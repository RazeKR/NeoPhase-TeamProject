using UnityEngine;

public class TestingRigidbodyCS : MonoBehaviour
{
	private GameObject ball1;

	private void Start()
	{
		ball1 = GameObject.Find("Sphere1");
		LeanTween.rotateAround(ball1, Vector3.forward, -90f, 1f);
		LeanTween.move(ball1, new Vector3(2f, 0f, 7f), 1f).setDelay(1f).setRepeat(-1);
	}

	private void Update()
	{
	}
}
