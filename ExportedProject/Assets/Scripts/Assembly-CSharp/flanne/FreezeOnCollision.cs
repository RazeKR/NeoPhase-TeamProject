using UnityEngine;

namespace flanne
{
	public class FreezeOnCollision : MonoBehaviour
	{
		[SerializeField]
		private string hitTag;

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(hitTag))
			{
				FreezeSystem.SharedInstance.Freeze(other.gameObject);
			}
		}
	}
}
