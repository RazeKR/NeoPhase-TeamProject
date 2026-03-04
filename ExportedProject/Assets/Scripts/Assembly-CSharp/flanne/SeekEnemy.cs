using UnityEngine;

namespace flanne
{
	public class SeekEnemy : MonoBehaviour
	{
		[SerializeField]
		private MoveComponent2D moveComponent;

		public float acceleration;

		[SerializeField]
		private float seekDistanceX;

		[SerializeField]
		private float seekDistanceY;

		public Transform player;

		private Transform _target;

		private void Start()
		{
			player = PlayerController.Instance.transform;
			GetNewTarget();
		}

		private void FixedUpdate()
		{
			if (_target != null)
			{
				Vector2 vector = base.transform.position;
				Vector2 vector2 = _target.position;
				if (Vector2.Distance(vector2, vector) < 1f)
				{
					GetNewTarget();
				}
				Vector2 vector3 = vector2 - vector;
				moveComponent.vector += vector3.normalized * acceleration * Time.fixedDeltaTime;
			}
			else
			{
				GetNewTarget();
			}
		}

		private void GetNewTarget()
		{
			Vector2 center = player.transform.position;
			Vector2 range = new Vector2(seekDistanceX, seekDistanceY);
			GameObject randomEnemy = EnemyFinder.GetRandomEnemy(center, range);
			if (randomEnemy == null)
			{
				_target = player;
			}
			else
			{
				_target = randomEnemy.transform;
			}
		}
	}
}
