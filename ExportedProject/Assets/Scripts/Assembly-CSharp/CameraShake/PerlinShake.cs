using System;
using UnityEngine;

namespace CameraShake
{
	public class PerlinShake : ICameraShake
	{
		[Serializable]
		public class Params
		{
			[Tooltip("Strength of the shake for each axis.")]
			public Displacement strength = new Displacement(Vector3.zero, new Vector3(2f, 2f, 0.8f));

			[Tooltip("Layers of perlin noise with different frequencies.")]
			public NoiseMode[] noiseModes = new NoiseMode[1]
			{
				new NoiseMode(12f, 1f)
			};

			[Tooltip("Strength of the shake over time.")]
			public Envelope.EnvelopeParams envelope;

			[Tooltip("How strength falls with distance from the shake source.")]
			public Attenuator.StrengthAttenuationParams attenuation;
		}

		[Serializable]
		public struct NoiseMode
		{
			[Tooltip("Frequency multiplier for the noise.")]
			public float freq;

			[Tooltip("Amplitude of the mode.")]
			[Range(0f, 1f)]
			public float amplitude;

			public NoiseMode(float freq, float amplitude)
			{
				this.freq = freq;
				this.amplitude = amplitude;
			}
		}

		private readonly Params pars;

		private readonly Envelope envelope;

		public IAmplitudeController AmplitudeController;

		private Vector2[] seeds;

		private float time;

		private Vector3? sourcePosition;

		private float norm;

		public Displacement CurrentDisplacement { get; private set; }

		public bool IsFinished { get; private set; }

		public PerlinShake(Params parameters, float maxAmplitude = 1f, Vector3? sourcePosition = null, bool manualStrengthControl = false)
		{
			pars = parameters;
			envelope = new Envelope(pars.envelope, maxAmplitude, manualStrengthControl ? Envelope.EnvelopeControlMode.Manual : Envelope.EnvelopeControlMode.Auto);
			AmplitudeController = envelope;
			this.sourcePosition = sourcePosition;
		}

		public void Initialize(Vector3 cameraPosition, Quaternion cameraRotation)
		{
			seeds = new Vector2[pars.noiseModes.Length];
			norm = 0f;
			for (int i = 0; i < seeds.Length; i++)
			{
				seeds[i] = UnityEngine.Random.insideUnitCircle * 20f;
				norm += pars.noiseModes[i].amplitude;
			}
		}

		public void Update(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
		{
			if (envelope.IsFinished)
			{
				IsFinished = true;
				return;
			}
			time += deltaTime;
			envelope.Update(deltaTime);
			Displacement zero = Displacement.Zero;
			for (int i = 0; i < pars.noiseModes.Length; i++)
			{
				zero += pars.noiseModes[i].amplitude / norm * SampleNoise(seeds[i], pars.noiseModes[i].freq);
			}
			CurrentDisplacement = envelope.Intensity * Displacement.Scale(zero, pars.strength);
			if (sourcePosition.HasValue)
			{
				CurrentDisplacement *= Attenuator.Strength(pars.attenuation, sourcePosition.Value, cameraPosition);
			}
		}

		private Displacement SampleNoise(Vector2 seed, float freq)
		{
			Vector3 position = new Vector3(Mathf.PerlinNoise(seed.x + time * freq, seed.y), Mathf.PerlinNoise(seed.x, seed.y + time * freq), Mathf.PerlinNoise(seed.x + time * freq, seed.y + time * freq)) - Vector3.one * 0.5f;
			Vector3 eulerAngles = new Vector3(Mathf.PerlinNoise(0f - seed.x - time * freq, 0f - seed.y), Mathf.PerlinNoise(0f - seed.x, 0f - seed.y - time * freq), Mathf.PerlinNoise(0f - seed.x - time * freq, 0f - seed.y - time * freq));
			eulerAngles -= Vector3.one * 0.5f;
			return new Displacement(position, eulerAngles);
		}
	}
}
