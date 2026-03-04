using System;
using UnityEngine;

namespace CameraShake
{
	public class KickShake : ICameraShake
	{
		[Serializable]
		public class Params
		{
			[Tooltip("Strength of the shake for each axis.")]
			public Displacement strength = new Displacement(Vector3.zero, Vector3.one);

			[Tooltip("How long it takes to move forward.")]
			public float attackTime = 0.05f;

			public AnimationCurve attackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

			[Tooltip("How long it takes to move back.")]
			public float releaseTime = 0.2f;

			public AnimationCurve releaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

			[Tooltip("How strength falls with distance from the shake source.")]
			public Attenuator.StrengthAttenuationParams attenuation;
		}

		private readonly Params pars;

		private readonly Vector3? sourcePosition;

		private readonly bool attenuateStrength;

		private Displacement direction;

		private Displacement prevWaypoint;

		private Displacement currentWaypoint;

		private bool release;

		private float t;

		public Displacement CurrentDisplacement { get; private set; }

		public bool IsFinished { get; private set; }

		public KickShake(Params parameters, Vector3 sourcePosition, bool attenuateStrength)
		{
			pars = parameters;
			this.sourcePosition = sourcePosition;
			this.attenuateStrength = attenuateStrength;
		}

		public KickShake(Params parameters, Displacement direction)
		{
			pars = parameters;
			this.direction = direction.Normalized;
		}

		public void Initialize(Vector3 cameraPosition, Quaternion cameraRotation)
		{
			if (sourcePosition.HasValue)
			{
				direction = Attenuator.Direction(sourcePosition.Value, cameraPosition, cameraRotation);
				if (attenuateStrength)
				{
					direction *= Attenuator.Strength(pars.attenuation, sourcePosition.Value, cameraPosition);
				}
			}
			currentWaypoint = Displacement.Scale(direction, pars.strength);
		}

		public void Update(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
		{
			if (t < 1f)
			{
				Move(deltaTime, release ? pars.releaseTime : pars.attackTime, release ? pars.releaseCurve : pars.attackCurve);
				return;
			}
			CurrentDisplacement = currentWaypoint;
			prevWaypoint = currentWaypoint;
			if (release)
			{
				IsFinished = true;
				return;
			}
			release = true;
			t = 0f;
			currentWaypoint = Displacement.Zero;
		}

		private void Move(float deltaTime, float duration, AnimationCurve curve)
		{
			if (duration > 0f)
			{
				t += deltaTime / duration;
			}
			else
			{
				t = 1f;
			}
			CurrentDisplacement = Displacement.Lerp(prevWaypoint, currentWaypoint, curve.Evaluate(t));
		}
	}
}
