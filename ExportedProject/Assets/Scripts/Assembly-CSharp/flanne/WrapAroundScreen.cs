using UnityEngine;

namespace flanne
{
	public class WrapAroundScreen : MonoBehaviour
	{
		[SerializeField]
		private int numWraps = 2;

		private Camera mainCamera;

		private int _ctr;

		private void Start()
		{
			mainCamera = Camera.main;
		}

		private void Update()
		{
			if (_ctr < numWraps)
			{
				WarpAround();
			}
		}

		private void OnDisable()
		{
			_ctr = 0;
		}

		private void WarpAround()
		{
			Vector3 position = mainCamera.WorldToViewportPoint(base.transform.position);
			bool flag = false;
			if ((double)position.x < -0.1)
			{
				flag = true;
				position.x = 1f;
			}
			else if ((double)position.x > 1.1)
			{
				flag = true;
				position.x = 0f;
			}
			if ((double)position.y < -0.1)
			{
				flag = true;
				position.y = 1f;
			}
			else if ((double)position.y > 1.1)
			{
				flag = true;
				position.y = 0f;
			}
			if (flag)
			{
				base.transform.position = mainCamera.ViewportToWorldPoint(position);
				_ctr++;
			}
		}
	}
}
