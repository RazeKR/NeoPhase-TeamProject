using UnityEngine;

namespace flanne
{
	public class SetRotationToOnEnable : MonoBehaviour
	{
		[SerializeField]
		private Transform targetTransform;

		private void OnEnable()
		{
			base.transform.rotation = targetTransform.rotation;
		}
	}
}
