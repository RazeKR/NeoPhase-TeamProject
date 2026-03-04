using UnityEngine;

namespace flanne
{
	public class Orbital : MonoBehaviour
	{
		public Transform center;

		public Vector3 axis = Vector3.up;

		public float radius = 2f;

		public float radiusSpeed = 0.5f;

		public float rotationSpeed = 80f;

		public bool dontRotate;

		private void Start()
		{
			if (center == null)
			{
				center = base.transform.parent;
			}
			base.transform.position = (base.transform.position - center.position).normalized * radius + center.position;
		}

		private void Update()
		{
			base.transform.RotateAround(center.position, axis, rotationSpeed * Time.deltaTime);
			Vector3 target = (base.transform.position - center.position).normalized * radius + center.position;
			base.transform.position = Vector3.MoveTowards(base.transform.position, target, Time.deltaTime * radiusSpeed);
			if (dontRotate)
			{
				base.transform.rotation = Quaternion.identity;
			}
		}
	}
}
