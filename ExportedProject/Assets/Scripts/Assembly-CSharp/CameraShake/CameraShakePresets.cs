using UnityEngine;

namespace CameraShake
{
	public class CameraShakePresets
	{
		private readonly CameraShaker shaker;

		public CameraShakePresets(CameraShaker shaker)
		{
			this.shaker = shaker;
		}

		public void ShortShake2D(float positionStrength = 0.08f, float rotationStrength = 0.1f, float freq = 25f, int numBounces = 5)
		{
			BounceShake.Params parameters = new BounceShake.Params
			{
				positionStrength = positionStrength,
				rotationStrength = rotationStrength,
				freq = freq,
				numBounces = numBounces
			};
			shaker.RegisterShake(new BounceShake(parameters));
		}

		public void ShortShake3D(float strength = 0.3f, float freq = 25f, int numBounces = 5)
		{
			BounceShake.Params parameters = new BounceShake.Params
			{
				axesMultiplier = new Displacement(Vector3.zero, new Vector3(1f, 1f, 0.4f)),
				rotationStrength = strength,
				freq = freq,
				numBounces = numBounces
			};
			shaker.RegisterShake(new BounceShake(parameters));
		}

		public void Explosion2D(float positionStrength = 1f, float rotationStrength = 3f, float duration = 0.5f)
		{
			PerlinShake.NoiseMode[] noiseModes = new PerlinShake.NoiseMode[2]
			{
				new PerlinShake.NoiseMode(8f, 1f),
				new PerlinShake.NoiseMode(20f, 0.3f)
			};
			Envelope.EnvelopeParams envelopeParams = new Envelope.EnvelopeParams();
			envelopeParams.decay = ((duration <= 0f) ? 1f : (1f / duration));
			PerlinShake.Params parameters = new PerlinShake.Params
			{
				strength = new Displacement(new Vector3(1f, 1f) * positionStrength, Vector3.forward * rotationStrength),
				noiseModes = noiseModes,
				envelope = envelopeParams
			};
			shaker.RegisterShake(new PerlinShake(parameters));
		}

		public void Explosion3D(float strength = 8f, float duration = 0.7f)
		{
			PerlinShake.NoiseMode[] noiseModes = new PerlinShake.NoiseMode[2]
			{
				new PerlinShake.NoiseMode(6f, 1f),
				new PerlinShake.NoiseMode(20f, 0.2f)
			};
			Envelope.EnvelopeParams envelopeParams = new Envelope.EnvelopeParams();
			envelopeParams.decay = ((duration <= 0f) ? 1f : (1f / duration));
			PerlinShake.Params parameters = new PerlinShake.Params
			{
				strength = new Displacement(Vector3.zero, new Vector3(1f, 1f, 0.5f) * strength),
				noiseModes = noiseModes,
				envelope = envelopeParams
			};
			shaker.RegisterShake(new PerlinShake(parameters));
		}
	}
}
