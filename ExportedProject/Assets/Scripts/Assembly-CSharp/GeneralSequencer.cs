using UnityEngine;

public class GeneralSequencer : MonoBehaviour
{
	public GameObject avatar1;

	public GameObject star;

	public GameObject dustCloudPrefab;

	public float speedScale = 1f;

	public void Start()
	{
		LTSeq lTSeq = LeanTween.sequence();
		lTSeq.append(LeanTween.moveY(avatar1, avatar1.transform.localPosition.y + 6f, 1f).setEaseOutQuad());
		lTSeq.insert(LeanTween.alpha(star, 0f, 1f));
		lTSeq.insert(LeanTween.scale(star, Vector3.one * 3f, 1f));
		lTSeq.append(LeanTween.rotateAround(avatar1, Vector3.forward, 360f, 0.6f).setEaseInBack());
		lTSeq.append(LeanTween.moveY(avatar1, avatar1.transform.localPosition.y, 1f).setEaseInQuad());
		lTSeq.append(delegate
		{
			for (int i = 0; (float)i < 50f; i++)
			{
				GameObject gameObject = Object.Instantiate(dustCloudPrefab);
				gameObject.transform.parent = avatar1.transform;
				gameObject.transform.localPosition = new Vector3(Random.Range(-2f, 2f), 0f, 0f);
				gameObject.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
				Vector3 to = new Vector3(gameObject.transform.localPosition.x, Random.Range(2f, 4f), Random.Range(-10f, 10f));
				LeanTween.moveLocal(gameObject, to, 3f * speedScale).setEaseOutCirc();
				LeanTween.rotateAround(gameObject, Vector3.forward, 720f, 3f * speedScale).setEaseOutCirc();
				LeanTween.alpha(gameObject, 0f, 3f * speedScale).setEaseOutCirc().setDestroyOnComplete(doesDestroy: true);
			}
		});
		lTSeq.setScale(speedScale);
	}
}
