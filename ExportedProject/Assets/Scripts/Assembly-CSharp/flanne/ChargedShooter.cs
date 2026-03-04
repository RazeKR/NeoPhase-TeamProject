using UnityEngine;

namespace flanne
{
	public class ChargedShooter : Shooter
	{
		[SerializeField]
		private Transform chargeUpSprite;

		[SerializeField]
		private GameObject maxChargeAnimationObj;

		[SerializeField]
		private int maxCharge;

		[SerializeField]
		private SoundEffectSO chargeUpSFX;

		[SerializeField]
		private SoundEffectSO maxChargeSFX;

		private int _charge = -1;

		public override bool fireOnStop => true;

		public override void OnStopShoot(ProjectileRecipe recipe, Vector2 pointDirection, int numProjectiles, float spread, float inaccuracy)
		{
			recipe.damage *= 1f + (float)_charge * 0.5f;
			recipe.size *= 1f + (float)_charge * 0.25f;
			if (_charge == maxCharge)
			{
				recipe.piercing += 999;
			}
			else
			{
				recipe.piercing += _charge;
			}
			base.Shoot(recipe, pointDirection, numProjectiles, spread, inaccuracy);
			_charge = -1;
			chargeUpSprite.localScale = Vector3.zero;
			maxChargeAnimationObj.SetActive(value: false);
			onShoot?.Invoke();
		}

		public override void Shoot(ProjectileRecipe recipe, Vector2 pointDirection, int numProjectiles, float spread, float inaccuracy)
		{
			if (_charge != maxCharge)
			{
				_charge++;
				chargeUpSprite.localScale = Vector3.one * ((float)_charge / (float)maxCharge);
				if (_charge == 0)
				{
					chargeUpSFX?.Play();
				}
				if (_charge == maxCharge)
				{
					maxChargeAnimationObj.SetActive(value: true);
					maxChargeSFX?.Play();
				}
			}
		}
	}
}
