using UnityEngine;

namespace flanne
{
	public class CurseOnCollision : MonoBehaviour
	{
		private CurseSystem CS;

		private void Start()
		{
			CS = CurseSystem.Instance;
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			CS.Curse(other.gameObject);
		}
	}
}
