using System.Collections;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class InvincibleAction : Action
	{
		[SerializeField]
		private float duration;

		private PlayerController player;

		public override void Init()
		{
			player = PlayerController.Instance;
		}

		public override void Activate(GameObject target)
		{
			player.StartCoroutine(StartInvincible());
		}

		private IEnumerator StartInvincible()
		{
			player.playerHealth.isInvincible.Flip();
			yield return new WaitForSeconds(duration);
			player.playerHealth.isInvincible.UnFlip();
		}
	}
}
