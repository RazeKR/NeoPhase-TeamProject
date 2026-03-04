using UnityEngine;

namespace flanne.UI
{
	public class DespawnRange : MonoBehaviour
	{
		private void OnCollisionExit2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains("Enemy") && !other.gameObject.tag.Contains("Champion") && !other.gameObject.tag.Contains("Passive"))
			{
				other.gameObject.SetActive(value: false);
			}
		}
	}
}
