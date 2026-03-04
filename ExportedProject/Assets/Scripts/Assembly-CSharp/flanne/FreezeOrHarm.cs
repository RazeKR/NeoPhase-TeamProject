using UnityEngine;

namespace flanne
{
	public class FreezeOrHarm : Harmful
	{
		[SerializeField]
		private string hitTag;

		private FreezeSystem freezeSys;

		private void Start()
		{
			freezeSys = FreezeSystem.SharedInstance;
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (!other.gameObject.tag.Contains(hitTag))
			{
				return;
			}
			if (!freezeSys.IsFrozen(other.gameObject))
			{
				freezeSys.Freeze(other.gameObject);
				return;
			}
			Health component = other.gameObject.GetComponent<Health>();
			if (component != null)
			{
				component.TakeDamage(DamageType.None, damageAmount);
			}
		}
	}
}
