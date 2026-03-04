using UnityEngine;

namespace flanne
{
	public class EnemyAreaProjectile : MonoBehaviour
	{
		[SerializeField]
		private Transform lobTransform;

		[SerializeField]
		private GameObject indicatorPrefab;

		[SerializeField]
		private GameObject damagePrefab;

		[SerializeField]
		private SoundEffectSO hitSFX;

		private GameObject _indicator;

		public void TargetPos(Vector2 pos, float duration)
		{
			lobTransform.LeanMoveLocalY(2f, duration / 2f).setLoopPingPong(1).setEase(LeanTweenType.easeOutCubic);
			lobTransform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
			LeanTween.move(base.gameObject, pos, duration).setOnComplete(OnTargetReached);
			_indicator = Object.Instantiate(indicatorPrefab);
			_indicator.transform.position = pos;
		}

		private void OnTargetReached()
		{
			hitSFX?.Play();
			Object.Instantiate(damagePrefab).transform.position = _indicator.transform.position;
			if ((bool)_indicator)
			{
				Object.Destroy(_indicator);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
