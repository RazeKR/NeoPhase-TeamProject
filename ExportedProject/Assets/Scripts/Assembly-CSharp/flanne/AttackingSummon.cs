using UnityEngine;

namespace flanne
{
	public abstract class AttackingSummon : Summon
	{
		public static string TweakAttackCDNotification = "AttackingSummon.TweakAttackCDNotification";

		public StatMod attackSpeedMod;

		[SerializeField]
		private float attackCooldown;

		[SerializeField]
		protected Animator animator;

		[SerializeField]
		private SoundEffectSO attackSoundFX;

		private float _timer;

		protected float finalAttackCooldown
		{
			get
			{
				float value = attackSpeedMod.ModifyInverse(base.summonAtkSpdMod.ModifyInverse(attackCooldown));
				value = value.NotifyModifiers(TweakAttackCDNotification, this);
				return Mathf.Max(0.1f, value);
			}
		}

		private void Awake()
		{
			attackSpeedMod = new StatMod();
		}

		private void Update()
		{
			if (_timer >= finalAttackCooldown)
			{
				if (Attack())
				{
					_timer -= finalAttackCooldown;
					if (animator != null)
					{
						animator.SetTrigger("Attack");
					}
					if (attackSoundFX != null)
					{
						attackSoundFX.Play();
					}
				}
			}
			else
			{
				_timer += Time.deltaTime;
			}
		}

		protected abstract bool Attack();
	}
}
