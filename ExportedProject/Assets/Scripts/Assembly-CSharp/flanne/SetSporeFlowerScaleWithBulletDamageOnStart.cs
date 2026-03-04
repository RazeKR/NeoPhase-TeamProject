using UnityEngine;

namespace flanne
{
	public class SetSporeFlowerScaleWithBulletDamageOnStart : MonoBehaviour
	{
		private void Start()
		{
			PlayerController.Instance.GetComponentInChildren<SporeFlower>().multiplyByBulletDamage = true;
		}
	}
}
