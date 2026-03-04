using System;
using UnityEngine;

namespace CameraShake
{
	public class BounceShake : ICameraShake
	{
		[Serializable]
		public class Params
		{
			[Tooltip("Strength of the shake for positional axes.")]
			public float positionStrength = 0.05f;

			[Tooltip("Strength of the shake for rotational axes.")]
			public float rotationStrength = 0.1f;

			[Tooltip("Preferred direction of shaking.")]
			public Displacement axesMultiplier = new Displacement(Vector2.one, Vector3.forward);

			[Tooltip("Frequency of shaking.")]
			public float freq = 25f;

			[Tooltip("Number of vibrations before stop.")]
			public int numBounces = 5;

			[Range(0f, 1f)]
			[Tooltip("Randomness of motion.")]
			public float randomness = 0.5f;

			[Tooltip("How strength falls with distance from the shake source.")]
			public Attenuator.StrengthAttenuationParams attenuation;
		}

		private readonly Params pars;

		private readonly AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		private readonly Vector3? sourcePosition;

		private float attenuation = 1f;

		private Displacement direction;

		private Displacement previousWaypoint;

		private Displacement currentWaypoint;

		private int bounceIndex;

		private float t;

		public Displacement CurrentDisplacement { get; private set; }

		public bool IsFinished { get; private set; }

		public BounceShake(Params parameters, Vector3? sourcePosition = null)
		{
			this.sourcePosition = sourcePosition;
			pars = parameters;
			Displacement a = Displacement.InsideUnitSpheres();
			direction = Displacement.Scale(a, pars.axesMultiplier).Normalized;
		}

		public BounceShake(Params parameters, Displacement initialDirection, Vector3? sourcePosition = null)
		{
			this.sourcePosition = sourcePosition;
			pars = parameters;
			direction = Displacement.Scale(initialDirection, pars.axesMultiplier).Normalized;
		}

		public void Initialize(Vector3 cameraPosition, Quaternion cameraRotation)
		{
			attenuation = ((!sourcePosition.HasValue) ? 1f : Attenuator.Strength(pars.attenuation, sourcePosition.Value, cameraPosition));
			currentWaypoint = attenuation * direction.ScaledBy(pars.positionStrength, pars.rotationStrength);
		}

		public void Update(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
		{
			if (t < 1f)
			{
				t += deltaTime * pars.freq;
				if (pars.freq == 0f)
				{
					t = 1f;
				}
				CurrentDisplacement = Displacement.Lerp(previousWaypoint, currentWaypoint, moveCurve.Evaluate(t));
				return;
			}
			t = 0f;
			CurrentDisplacement = currentWaypoint;
			previousWaypoint = currentWaypoint;
			bounceIndex++;
			if (bounceIndex > pars.numBounces)
			{
				IsFinished = true;
				return;
			}
			Displacement a = Displacement.InsideUnitSpheres();
			direction = -direction + pars.randomness * Displacement.Scale(a, pars.axesMultiplier).Normalized;
			direction = direction.Normalized;
			float num = 1f - (float)bounceIndex / (float)pars.numBounces;
			currentWaypoint = num * num * attenuation * direction.ScaledBy(pars.positionStrength, pars.rotationStrength);
		}
	}
}
