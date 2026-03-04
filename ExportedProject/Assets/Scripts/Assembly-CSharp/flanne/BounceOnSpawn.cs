using UnityEngine;

namespace flanne
{
	public class BounceOnSpawn : MonoBehaviour
	{
		[SerializeField]
		private float bounceHeight;

		[SerializeField]
		private float duration;

		private Vector3 _startPos;

		private int _tweenID;

		private void OnEnable()
		{
			_startPos = base.transform.localPosition;
			Vector3 to = _startPos + new Vector3(0f, bounceHeight, 0f);
			_tweenID = LeanTween.moveLocal(base.gameObject, to, duration / 4f).setEase(LeanTweenType.easeOutSine).setOnComplete(Fall)
				.id;
		}

		private void OnDisable()
		{
			if (LeanTween.isTweening(_tweenID))
			{
				LeanTween.cancel(_tweenID);
			}
		}

		private void Fall()
		{
			_tweenID = LeanTween.moveLocal(base.gameObject, _startPos, duration * 0.75f).setEase(LeanTweenType.easeOutBounce).id;
		}
	}
}
