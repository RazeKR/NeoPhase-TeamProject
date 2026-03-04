using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class AreaProjectile : MonoBehaviour
	{
		[SerializeField]
		private Transform lobTransform;

		public UnityEvent onTargetReached;

		public void TargetPos(Vector2 pos, float duration)
		{
			lobTransform.LeanMoveLocalY(2f, duration / 2f).setLoopPingPong(1).setEase(LeanTweenType.easeOutCubic);
			lobTransform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
			LeanTween.move(base.gameObject, pos, duration).setOnComplete(OnTargetReached);
		}

		private void OnTargetReached()
		{
			onTargetReached.Invoke();
			base.gameObject.SetActive(value: false);
		}
	}
}
