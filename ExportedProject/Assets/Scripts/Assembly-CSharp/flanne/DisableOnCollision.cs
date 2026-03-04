using UnityEngine;

namespace flanne
{
	public class DisableOnCollision : MonoBehaviour
	{
		private void OnCollisionEnter2D(Collision2D collision)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
