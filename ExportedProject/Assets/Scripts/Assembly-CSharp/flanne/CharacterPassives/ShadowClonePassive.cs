using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace flanne.CharacterPassives
{
	public class ShadowClonePassive : SkillPassive
	{
		[SerializeField]
		private float dashSpeedMulti;

		[SerializeField]
		private float dashDuration;

		[SerializeField]
		private GameObject shadowClone;

		[SerializeField]
		private TimeToLive shadowCloneLifetime;

		[SerializeField]
		private SpriteTrail spriteTrail;

		public UnityEvent onUse;

		public UnityEvent onEnd;

		private PlayerController player;

		protected override void Init()
		{
			player = base.transform.root.GetComponent<PlayerController>();
			spriteTrail.mLeadingSprite = player.playerSprite;
			shadowClone.SetActive(value: false);
		}

		protected override void PerformSkill()
		{
			StartCoroutine(DashCR(player));
			SummonShadowClone();
			onUse?.Invoke();
		}

		private void SummonShadowClone()
		{
			if (!(shadowClone == null))
			{
				shadowClone.transform.position = player.transform.position;
				shadowClone.SetActive(value: true);
				shadowCloneLifetime.Refresh();
			}
		}

		private IEnumerator DashCR(PlayerController player)
		{
			player.disableAction.Flip();
			player.playerHealth.isInvincible.Flip();
			float originalMoveSpeed = player.movementSpeed;
			player.movementSpeed *= dashSpeedMulti;
			spriteTrail?.SetEnabled(enabled: true);
			yield return new WaitForSeconds(dashDuration);
			player.movementSpeed = originalMoveSpeed;
			spriteTrail?.SetEnabled(enabled: false);
			player.disableAction.UnFlip();
			onEnd?.Invoke();
			yield return new WaitForSeconds(0.3f);
			player.playerHealth.isInvincible.UnFlip();
		}
	}
}
