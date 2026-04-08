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

        public bool movable = true;

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

        private void FixedUpdate()
        {
            if (_vector == Vector2.zero) return;

            if (!movable) return;

            if (drag > 0)
            {
                if (_vector.magnitude < 0.01f) _vector = Vector2.zero;
                else _vector -= _vector * drag * Time.fixedDeltaTime;
            }

            Rb.MovePosition(Rb.position + _vector * Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            vector = Vector2.zero;
            Rb.velocity = Vector2.zero;
            Rb.angularVelocity = 0f;
        }
    }
}
