using System;
using UnityEngine;

namespace CameraShake
{
	public static class Attenuator
	{
		[Serializable]
		public class StrengthAttenuationParams
		{
			[Tooltip("Radius in which shake doesn't lose strength.")]
			public float clippingDistance = 10f;

			[Tooltip("How fast strength falls with distance.")]
			public float falloffScale = 50f;

			[Tooltip("Power of the falloff function.")]
			public Degree falloffDegree = Degree.Quadratic;

			[Tooltip("Contribution of each axis to distance. E. g. (1, 1, 0) for a 2D game in XY plane.")]
			public Vector3 axesMultiplier = Vector3.one;
		}

		public static float Strength(StrengthAttenuationParams pars, Vector3 sourcePosition, Vector3 cameraPosition)
		{
			Vector3 b = cameraPosition - sourcePosition;
			float magnitude = Vector3.Scale(pars.axesMultiplier, b).magnitude;
			return Power.Evaluate(Mathf.Clamp01(1f - (magnitude - pars.clippingDistance) / pars.falloffScale), pars.falloffDegree);
		}

		public static Displacement Direction(Vector3 sourcePosition, Vector3 cameraPosition, Quaternion cameraRotation)
		{
			Displacement zero = Displacement.Zero;
			zero.position = (cameraPosition - sourcePosition).normalized;
			zero.position = Quaternion.Inverse(cameraRotation) * zero.position;
			zero.eulerAngles.x = zero.position.z;
			zero.eulerAngles.y = zero.position.x;
			zero.eulerAngles.z = 0f - zero.position.x;
			return zero;
		}
	}
}
