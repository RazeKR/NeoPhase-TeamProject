using System.Collections.Generic;
using UnityEngine;

namespace CameraShake
{
	public class CameraShaker : MonoBehaviour
	{
		public static CameraShaker Instance;

		public static CameraShakePresets Presets;

		public static bool ShakeOn;

		private readonly List<ICameraShake> activeShakes = new List<ICameraShake>();

		[Tooltip("Transform which will be affected by the shakes.\n\nCameraShaker will set this transform's local position and rotation.")]
		[SerializeField]
		private Transform cameraTransform;

		[Tooltip("Scales the strength of all shakes.")]
		[Range(0f, 1f)]
		[SerializeField]
		public float StrengthMultiplier = 1f;

		public CameraShakePresets ShakePresets;

		public static void Shake(ICameraShake shake)
		{
			if (!IsInstanceNull())
			{
				Instance.RegisterShake(shake);
			}
		}

		public void RegisterShake(ICameraShake shake)
		{
			shake.Initialize(cameraTransform.position, cameraTransform.rotation);
			activeShakes.Add(shake);
		}

		public void SetCameraTransform(Transform cameraTransform)
		{
			cameraTransform.localPosition = Vector3.zero;
			cameraTransform.localEulerAngles = Vector3.zero;
			this.cameraTransform = cameraTransform;
		}

		private void Awake()
		{
			Instance = this;
			ShakePresets = new CameraShakePresets(this);
			Presets = ShakePresets;
			if (cameraTransform == null)
			{
				cameraTransform = base.transform;
			}
		}

		private void Update()
		{
			if (cameraTransform == null || !ShakeOn)
			{
				return;
			}
			Displacement zero = Displacement.Zero;
			for (int num = activeShakes.Count - 1; num >= 0; num--)
			{
				if (activeShakes[num].IsFinished)
				{
					activeShakes.RemoveAt(num);
				}
				else
				{
					activeShakes[num].Update(Time.deltaTime, cameraTransform.position, cameraTransform.rotation);
					zero += activeShakes[num].CurrentDisplacement;
				}
			}
			cameraTransform.localPosition = StrengthMultiplier * zero.position;
			cameraTransform.localRotation = Quaternion.Euler(StrengthMultiplier * zero.eulerAngles);
		}

		private static bool IsInstanceNull()
		{
			if (Instance == null)
			{
				Debug.LogError("CameraShaker Instance is missing!");
				return true;
			}
			return false;
		}
	}
}
