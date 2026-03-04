using UnityEngine;

namespace flanne
{
	public class VerticleBounce : MonoBehaviour
	{
		[SerializeField]
		private float bounceHeight;

		[SerializeField]
		private float bouncePerSecond;

		private int _tweenID;

		private void OnEnable()
		{
			float to = base.transform.localPosition.y + bounceHeight;
			_tweenID = LeanTween.moveLocalY(base.gameObject, to, 1f / (bouncePerSecond / 2f)).setLoopPingPong().id;
		}

		private void OnDisable()
		{
			if (LeanTween.isTweening(_tweenID))
			{
				LeanTween.cancel(_tweenID);
			}
		}
	}
}
