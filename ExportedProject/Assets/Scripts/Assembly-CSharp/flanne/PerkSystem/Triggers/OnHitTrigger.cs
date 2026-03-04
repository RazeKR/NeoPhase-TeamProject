using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnHitTrigger : Trigger
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float triggerChance = 1f;

		[SerializeField]
		private bool actionTargetPlayer;

		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			PlayerController instance = PlayerController.Instance;
			if (!(gameObject == instance) && Random.Range(0f, 1f) < triggerChance)
			{
				if (actionTargetPlayer)
				{
					RaiseTrigger(instance.gameObject);
				}
				else
				{
					RaiseTrigger(gameObject);
				}
			}
		}
	}
}
