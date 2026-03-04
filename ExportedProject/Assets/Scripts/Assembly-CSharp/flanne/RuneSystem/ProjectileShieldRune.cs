using System.Collections;
using UnityEngine;

namespace flanne.RuneSystem
{
	[RequireComponent(typeof(Collider2D))]
	public class ProjectileShieldRune : Rune
	{
		[SerializeField]
		private float baseCD = 10f;

		[SerializeField]
		private float cdrPerLevel = 1f;

		[SerializeField]
		private SpriteRenderer sprite;

		private Collider2D shieldCollider;

		protected override void Init()
		{
			shieldCollider = GetComponent<Collider2D>();
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			sprite.enabled = false;
			shieldCollider.enabled = false;
			StartCoroutine(WaitForCooldownCR());
		}

		private IEnumerator WaitForCooldownCR()
		{
			yield return new WaitForSeconds(baseCD - cdrPerLevel * (float)level);
			sprite.enabled = true;
			shieldCollider.enabled = true;
		}
	}
}
