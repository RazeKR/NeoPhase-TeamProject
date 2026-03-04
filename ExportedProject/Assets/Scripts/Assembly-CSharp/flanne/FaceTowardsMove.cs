using UnityEngine;

namespace flanne
{
	public class FaceTowardsMove : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer sprite;

		[SerializeField]
		private MoveComponent2D movement;

		private void Update()
		{
			sprite.flipX = movement.vector.x < 0f;
		}
	}
}
