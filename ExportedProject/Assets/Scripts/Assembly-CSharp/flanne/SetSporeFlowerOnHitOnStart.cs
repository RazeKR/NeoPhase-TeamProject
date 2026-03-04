using UnityEngine;

namespace flanne
{
	public class SetSporeFlowerOnHitOnStart : MonoBehaviour
	{
		private void Start()
		{
			PlayerController.Instance.GetComponentInChildren<SporeFlower>().procsOnHit = true;
		}
	}
}
