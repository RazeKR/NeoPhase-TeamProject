using UnityEngine;

namespace flanne
{
	public class StartHalfHP : MonoBehaviour
	{
		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			componentInParent.playerHealth.hp -= componentInParent.playerHealth.maxHP / 2;
		}
	}
}
