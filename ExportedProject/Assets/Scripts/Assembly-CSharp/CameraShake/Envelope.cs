using System;
using UnityEngine;

namespace CameraShake
{
	public class Envelope : IAmplitudeController
	{
		[Serializable]
		public class EnvelopeParams
		{
			[Tooltip("How fast the amplitude increases.")]
			public float attack = 10f;

			[Tooltip("How long in seconds the amplitude holds maximum value.")]
			public float sustain;

			[Tooltip("How fast the amplitude decreases.")]
			public float decay = 1f;

			[Tooltip("Power in which the amplitude is raised to get intensity.")]
			public Degree degree = Degree.Cubic;
		}

		public enum EnvelopeControlMode
		{
			Auto = 0,
			Manual = 1
		}

		public enum EnvelopeState
		{
			Sustain = 0,
			Increase = 1,
			Decrease = 2
		}

		private readonly EnvelopeParams pars;

		private readonly EnvelopeControlMode controlMode;

		private float amplitude;

		private float targetAmplitude;

		private float sustainEndTime;

		private bool finishWhenAmplitudeZero;

		private bool finishImmediately;

		private EnvelopeState state;

		public float Intensity { get; private set; }

		public bool IsFinished
		{
			get
			{
				if (finishImmediately)
				{
					return true;
				}
				if ((finishWhenAmplitudeZero || controlMode == EnvelopeControlMode.Auto) && amplitude <= 0f)
				{
					return targetAmplitude <= 0f;
				}
				return false;
			}
		}

		public Envelope(EnvelopeParams pars, float initialTargetAmplitude, EnvelopeControlMode controlMode)
		{
			this.pars = pars;
			this.controlMode = controlMode;
			SetTarget(initialTargetAmplitude);
		}

		public void Finish()
		{
			finishWhenAmplitudeZero = true;
			SetTarget(0f);
		}

		public void FinishImmediately()
		{
			finishImmediately = true;
		}

		public void Update(float deltaTime)
		{
			if (IsFinished)
			{
				return;
			}
			if (state == EnvelopeState.Increase)
			{
				if (pars.attack > 0f)
				{
					amplitude += deltaTime * pars.attack;
				}
				if (amplitude > targetAmplitude || pars.attack <= 0f)
				{
					amplitude = targetAmplitude;
					state = EnvelopeState.Sustain;
					if (controlMode == EnvelopeControlMode.Auto)
					{
						sustainEndTime = Time.time + pars.sustain;
					}
				}
			}
			else if (state == EnvelopeState.Decrease)
			{
				if (pars.decay > 0f)
				{
					amplitude -= deltaTime * pars.decay;
				}
				if (amplitude < targetAmplitude || pars.decay <= 0f)
				{
					amplitude = targetAmplitude;
					state = EnvelopeState.Sustain;
				}
			}
			else if (controlMode == EnvelopeControlMode.Auto && Time.time > sustainEndTime)
			{
				SetTarget(0f);
			}
			amplitude = Mathf.Clamp01(amplitude);
			Intensity = Power.Evaluate(amplitude, pars.degree);
		}

		public void SetTargetAmplitude(float value)
		{
			if (controlMode == EnvelopeControlMode.Manual && !finishWhenAmplitudeZero)
			{
				SetTarget(value);
			}
		}

		private void SetTarget(float value)
		{
			targetAmplitude = Mathf.Clamp01(value);
			state = ((targetAmplitude > amplitude) ? EnvelopeState.Increase : EnvelopeState.Decrease);
		}
	}
}
