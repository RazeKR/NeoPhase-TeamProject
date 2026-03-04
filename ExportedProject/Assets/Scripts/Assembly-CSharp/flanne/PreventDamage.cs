using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class PreventDamage : MonoBehaviour
	{
		public float cooldownTime;

		public StatMod cooldownRate;

		public UnityEvent OnDamagePrevented;

		public UnityEvent OnCooldownDone;

		private PlayerHealth playerHealth;

		private IEnumerator _waitCooldownCoroutine;

		private float _timer;

		public bool isActive { get; private set; }

		public float finalCooldown => cooldownRate.ModifyInverse(cooldownTime);

		private void OnPreventDamage()
		{
			if (_waitCooldownCoroutine == null)
			{
				_waitCooldownCoroutine = InvincibilityCooldownCR();
				StartCoroutine(_waitCooldownCoroutine);
			}
		}

		private void Start()
		{
			cooldownRate = new StatMod();
			_waitCooldownCoroutine = null;
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			playerHealth = componentInParent.playerHealth;
			playerHealth.onDamagePrevented.AddListener(OnPreventDamage);
			ApplyInvincibility();
		}

		private void OnDestroy()
		{
			playerHealth.onDamagePrevented.RemoveListener(OnPreventDamage);
		}

		public void ReduceCDTimer(float reduceAmount)
		{
			if (!isActive)
			{
				_timer += reduceAmount;
			}
		}

		private void ApplyInvincibility()
		{
			playerHealth.isProtected = true;
			isActive = true;
			OnCooldownDone.Invoke();
		}

		private IEnumerator InvincibilityCooldownCR()
		{
			OnDamagePrevented.Invoke();
			playerHealth.isInvincible.Flip();
			yield return new WaitForSeconds(0.5f);
			playerHealth.isInvincible.UnFlip();
			isActive = false;
			_timer = 0f;
			while (_timer < finalCooldown)
			{
				_timer += Time.deltaTime;
				yield return null;
			}
			ApplyInvincibility();
			_waitCooldownCoroutine = null;
		}
	}
}
