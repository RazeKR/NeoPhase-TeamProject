using System.Collections;
using UnityEngine;

namespace flanne.RuneSystem
{
	public class EtherealRune : Rune
	{
		[SerializeField]
		private float durationPerLevel;

		[SerializeField]
		private int killsToActivate;

		private int _counter;

		private bool _isActive;

		private void OnDeath(object sender, object args)
		{
			if (!_isActive && (sender as Health).gameObject.tag == "Enemy")
			{
				_counter++;
				if (_counter >= killsToActivate)
				{
					StartCoroutine(EtherealStateCR());
					_counter = 0;
				}
			}
		}

		protected override void Init()
		{
			_isActive = false;
			this.AddObserver(OnDeath, Health.DeathEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnDeath, Health.DeathEvent);
		}

		private IEnumerator EtherealStateCR()
		{
			_isActive = true;
			player.ammo.infiniteAmmo.Flip();
			yield return new WaitForSeconds(durationPerLevel * (float)level);
			_isActive = false;
			player.ammo.infiniteAmmo.UnFlip();
		}
	}
}
