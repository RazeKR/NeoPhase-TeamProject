using CameraShake;
using UnityEngine;

namespace flanne
{
	public class ShortShake2D : MonoBehaviour
	{
		[SerializeField]
		private float positionStrength;

		[SerializeField]
		private float rotationStrength;

		public void Shake()
		{
			CameraShaker.Presets.ShortShake2D(positionStrength, rotationStrength);
		}
	}
}
