using UnityEngine;

namespace flanne
{
	public class HomeTowardsPlayer : MonoBehaviour
	{
		[SerializeField]
		private MoveComponent2D moveComponent;

		[SerializeField]
		private float acceleration;

		private Transform playerTransform;

		private void Start()
		{
			playerTransform = PlayerController.Instance.transform;
		}

		private void FixedUpdate()
		{
			Vector2 vector = playerTransform.position - base.transform.position;
			moveComponent.vector += vector.normalized * acceleration * Time.fixedDeltaTime;
		}
	}
}
