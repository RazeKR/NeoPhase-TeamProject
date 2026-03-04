using UnityEngine;

namespace flanne
{
	public abstract class TieredUnlockable : MonoBehaviour
	{
		public abstract int level { get; set; }
	}
}
