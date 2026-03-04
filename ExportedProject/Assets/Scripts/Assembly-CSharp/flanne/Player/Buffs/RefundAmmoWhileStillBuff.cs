using System;
using System.Collections;
using UnityEngine;

namespace flanne.Player.Buffs
{
	public class RefundAmmoWhileStillBuff : Buff
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToRefund;

		[NonSerialized]
		private IEnumerator _checkStillCoroutine;

		[NonSerialized]
		private Vector3 _lastFramePos;

		[NonSerialized]
		private Vector3 _thisFramePos;

		public override void OnAttach()
		{
			this.AddObserver(OnCheckshouldConumeAmmo, Ammo.ShouldConsumeAmmoCheck);
			_checkStillCoroutine = CheckStillCR();
			PlayerController.Instance.StartCoroutine(_checkStillCoroutine);
		}

		public override void OnUnattach()
		{
			this.RemoveObserver(OnCheckshouldConumeAmmo, Ammo.ShouldConsumeAmmoCheck);
			PlayerController.Instance.StopCoroutine(_checkStillCoroutine);
		}

		private void OnCheckshouldConumeAmmo(object sender, object args)
		{
			BaseException ex = args as BaseException;
			if (_lastFramePos == _thisFramePos && UnityEngine.Random.Range(0f, 1f) < chanceToRefund)
			{
				ex.FlipToggle();
			}
		}

		private IEnumerator CheckStillCR()
		{
			PlayerController player = PlayerController.Instance;
			_lastFramePos = player.transform.position;
			_thisFramePos = player.transform.position;
			while (true)
			{
				yield return new WaitForFixedUpdate();
				_lastFramePos = _thisFramePos;
				_thisFramePos = player.transform.position;
			}
		}
	}
}
