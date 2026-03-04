using UnityEngine;

namespace flanne.Core
{
	public class CameraRig : MonoBehaviour
	{
		[SerializeField]
		private float maxLookDistance;

		private Transform parent;

		private ShootingCursor SC;

		private void Awake()
		{
			parent = base.transform.parent;
		}

		private void Start()
		{
			SC = ShootingCursor.Instance;
		}

		private void Update()
		{
			if (PauseController.isPaused)
			{
				return;
			}
			if (SC.autoAim || SC.usingGamepad)
			{
				base.transform.localPosition = Vector3.zero;
				return;
			}
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 vector2 = parent.position;
			Vector2 vector3 = vector - vector2;
			vector3 /= 12f;
			if (maxLookDistance < vector3.magnitude)
			{
				vector3 = vector3.normalized;
				vector3 = maxLookDistance * vector3;
			}
			base.transform.localPosition = vector3;
		}
	}
}
