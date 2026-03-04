using UnityEngine;

namespace flanne
{
	public class ScaleBounce : MonoBehaviour
	{
		[SerializeField]
		private Vector3 scaleTo;

		[SerializeField]
		private float bouncePerSecond;

		[SerializeField]
		private bool ignoreTimeScale;

		private int _tweenID;

		private void OnEnable()
		{
			_tweenID = LeanTween.scale(base.gameObject, scaleTo, 1f / (bouncePerSecond / 2f)).setLoopPingPong().setIgnoreTimeScale(ignoreTimeScale)
				.id;
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
