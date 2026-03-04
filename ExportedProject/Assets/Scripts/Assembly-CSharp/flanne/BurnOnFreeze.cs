using UnityEngine;

namespace flanne
{
	public class BurnOnFreeze : MonoBehaviour
	{
		[SerializeField]
		private int burnDamage;

		private BurnSystem BurnSys;

		private void Start()
		{
			this.AddObserver(OnFreeze, FreezeSystem.InflictFreezeEvent);
			BurnSys = BurnSystem.SharedInstance;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnFreeze, FreezeSystem.InflictFreezeEvent);
		}

		private void OnFreeze(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			if (gameObject.tag.Contains("Enemy"))
			{
				BurnSys.Burn(gameObject, burnDamage);
			}
		}
	}
}
