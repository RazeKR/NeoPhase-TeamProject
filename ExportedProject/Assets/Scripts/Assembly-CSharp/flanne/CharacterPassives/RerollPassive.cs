using UnityEngine;

namespace flanne.CharacterPassives
{
	public class RerollPassive : MonoBehaviour
	{
		private void Start()
		{
			PowerupGenerator.CanReroll = true;
		}
	}
}
