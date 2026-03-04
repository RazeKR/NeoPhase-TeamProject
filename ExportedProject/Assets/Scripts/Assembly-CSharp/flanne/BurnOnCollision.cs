using UnityEngine;

namespace flanne
{
	public class BurnOnCollision : MonoBehaviour
	{
		public string hitTag;

		public int burnDamage;

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(hitTag))
			{
				BurnSystem.SharedInstance.Burn(other.gameObject, burnDamage);
			}
		}
	}
}
