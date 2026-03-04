using UnityEngine;

namespace flanne
{
	public class BurnOnLightning : MonoBehaviour
	{
		private BurnSystem BS;

		private void Start()
		{
			BS = BurnSystem.SharedInstance;
			this.AddObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnThunderHit(object sender, object args)
		{
			GameObject target = args as GameObject;
			BS.Burn(target, 3);
		}
	}
}
