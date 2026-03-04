using UnityEngine;

namespace flanne.PowerupSystems
{
	public class NoBulletDamage : MonoBehaviour
	{
		private void Start()
		{
			StatsHolder stats = base.transform.GetComponentInParent<PlayerController>().stats;
			stats[StatType.BulletDamage].AddMultiplierReduction(0f);
			Debug.Log(stats[StatType.BulletDamage].Modify(1f));
		}
	}
}
