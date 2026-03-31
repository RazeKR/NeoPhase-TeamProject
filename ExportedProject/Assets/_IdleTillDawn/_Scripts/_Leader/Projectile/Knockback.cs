using UnityEngine;

namespace flanne
{
    public class Knockback : MonoBehaviour
    {
        public float knockbackForce;

        [SerializeField]
        private bool ignoreKBImmune;

        private MoveComponent2D myMove;

        private void Awake()
        {
            myMove = GetComponent<MoveComponent2D>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            MoveComponent2D component = other.gameObject.GetComponent<MoveComponent2D>();
            if (!(component == null) && (!component.knockbackImmune || ignoreKBImmune) && !other.gameObject.tag.Contains("Passive"))
            {
                if (myMove != null)
                {
                    component.vector = knockbackForce * myMove.vectorLastFrame.normalized;
                    return;
                }
                Vector2 vector = other.transform.position - base.transform.position;
                component.vector = knockbackForce * vector.normalized;
            }
        }
    }
}
