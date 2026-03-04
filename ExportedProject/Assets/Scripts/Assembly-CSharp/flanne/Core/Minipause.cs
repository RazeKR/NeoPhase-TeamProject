using UnityEngine;

namespace flanne.Core
{
	public class Minipause : MonoBehaviour
	{
		[SerializeField]
		private bool hasCooldown;

		[SerializeField]
		private float cd;

		private PauseController PC;

		private float _cdTimer;

		private void Start()
		{
			PC = PauseController.SharedInstance;
			_cdTimer = cd;
		}

		private void Update()
		{
			if (hasCooldown && _cdTimer > 0f)
			{
				_cdTimer -= Time.deltaTime;
			}
		}

		public void Pause(float duration)
		{
			if (hasCooldown)
			{
				if (_cdTimer <= 0f)
				{
					PC.Pause(duration);
				}
				_cdTimer = cd;
			}
			else
			{
				PC.Pause(duration);
			}
		}
	}
}
