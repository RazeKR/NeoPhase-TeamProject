using UnityEngine;

namespace flanne
{
	public class RotatorTowardsPlayerOnEnable : MonoBehaviour
	{
		private void OnEnable()
		{
			Vector2 vector = PlayerController.Instance.transform.position - base.transform.position;
			float angle = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			base.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
	}
}
