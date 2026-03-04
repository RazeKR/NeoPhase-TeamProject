using UnityEngine;

namespace flanne
{
	public class MoveComponent2D : MonoBehaviour
	{
		private Vector2 _vector;

		public Vector2 vectorLastFrame;

		public float drag;

		public Rigidbody2D Rb;

		public bool knockbackImmune;

		public bool rotateTowardsMove;

		public Vector2 vector
		{
			get
			{
				return _vector;
			}
			set
			{
				vectorLastFrame = _vector;
				_vector = value;
			}
		}

		private void Start()
		{
			MoveSystem2D.Register(this);
		}

		private void OnDisable()
		{
			vector = Vector2.zero;
			Rb.velocity = Vector2.zero;
			Rb.angularVelocity = 0f;
		}

		private void OnDestroy()
		{
			MoveSystem2D.UnRegister(this);
		}
	}
}
