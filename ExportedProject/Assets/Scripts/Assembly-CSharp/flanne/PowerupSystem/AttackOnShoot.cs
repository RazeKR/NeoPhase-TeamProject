using UnityEngine;

namespace flanne.PowerupSystem
{
	public abstract class AttackOnShoot : MonoBehaviour
	{
		[SerializeField]
		private SoundEffectSO soundFX;

		[SerializeField]
		private int shotsPerAttack;

		private int _counter;

		private Gun myGun;

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			myGun = componentInParent.gun;
			myGun.OnShoot.AddListener(IncrementCounter);
			Init();
		}

		private void OnDestroy()
		{
			myGun.OnShoot.RemoveListener(IncrementCounter);
		}

		public void IncrementCounter()
		{
			_counter++;
			if (_counter >= shotsPerAttack)
			{
				_counter = 0;
				Attack();
				soundFX?.Play();
			}
		}

		public abstract void Attack();

		protected virtual void Init()
		{
		}
	}
}
