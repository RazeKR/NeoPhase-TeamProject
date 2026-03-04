namespace CameraShake
{
	public interface IAmplitudeController
	{
		void SetTargetAmplitude(float value);

		void Finish();

		void FinishImmediately();
	}
}
