using CameraShake;
using UnityEngine;

namespace flanne
{
	public class ExplosionShake2D : MonoBehaviour
	{
		[SerializeField]
		private float positionStrength;

		[SerializeField]
		private float rotationStrength;

		public void Shake()
		{
			CameraShaker.Presets.Explosion2D(positionStrength, rotationStrength);
		}
	}
}
