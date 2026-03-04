using UnityEngine;

namespace flanne.PowerupSystem
{
	public class RefundAmmoWhenStanding : MonoBehaviour
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToRefund;

		private Vector3 _lastFramePos;

		private Vector3 _thisFramePos;

		private Gun myGun;

		private Ammo ammo;

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			myGun = componentInParent.gun;
			myGun.OnShoot.AddListener(ChangeToRefundAmmo);
			ammo = base.transform.parent.GetComponentInChildren<Ammo>();
			_lastFramePos = base.transform.position;
			_thisFramePos = base.transform.position;
		}

		private void OnDestroy()
		{
			myGun.OnShoot.RemoveListener(ChangeToRefundAmmo);
		}

		private void Update()
		{
			_lastFramePos = _thisFramePos;
			_thisFramePos = base.transform.position;
		}

		private void ChangeToRefundAmmo()
		{
			if (_lastFramePos == _thisFramePos && Random.Range(0f, 1f) < chanceToRefund)
			{
				ammo.GainAmmo();
			}
		}
	}
}
