using UnityEngine;

namespace flanne
{
	public class CurseAndHarmOnContact : MonoBehaviour
	{
		public float multiplier = 1f;

		private CurseSystem CS;

		private PlayerController player;

		private Gun playerGun;

		private void Start()
		{
			CS = CurseSystem.Instance;
			player = PlayerController.Instance;
			playerGun = player.gun;
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			CS.Curse(other.gameObject);
			Health component = other.gameObject.GetComponent<Health>();
			if (component != null)
			{
				component.TakeDamage(DamageType.Bullet, Mathf.FloorToInt(multiplier * playerGun.damage));
			}
		}
	}
}
