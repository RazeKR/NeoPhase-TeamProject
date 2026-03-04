using UnityEngine;

namespace flanne
{
	public class Spinning : MonoBehaviour
	{
		[SerializeField]
		private Vector3 spin = Vector3.zero;

		private void Update()
		{
			base.transform.Rotate(spin.x, spin.y, spin.z * Time.deltaTime);
		}
	}
}
