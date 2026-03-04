using UnityEngine;

namespace flanne
{
	public class HomingProjectile : MonoBehaviour
	{
		[SerializeField]
		private MoveComponent2D moveComponent;

		[SerializeField]
		private float acceleration;

		private GameObject _target;

		private void OnEnable()
		{
			_target = EnemyFinder.GetRandomEnemy(Vector3.zero, Vector3.positiveInfinity);
		}

		private void FixedUpdate()
		{
			if (_target != null && _target.activeSelf)
			{
				Vector2 vector = base.transform.position;
				Vector2 vector2 = (Vector2)_target.transform.position - vector;
				moveComponent.vector += vector2.normalized * acceleration * Time.fixedDeltaTime;
			}
			else
			{
				_target = EnemyFinder.GetRandomEnemy(Vector3.zero, Vector3.positiveInfinity);
			}
		}
	}
}
