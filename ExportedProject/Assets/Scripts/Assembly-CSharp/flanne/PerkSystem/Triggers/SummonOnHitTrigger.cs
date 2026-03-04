using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class SummonOnHitTrigger : Trigger
	{
		[SerializeField]
		private string summonTypeID;

		[Range(0f, 1f)]
		[SerializeField]
		private float triggerChance = 1f;

		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnHit, Summon.SummonOnHitNotification);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnHit, Summon.SummonOnHitNotification);
		}

		private void OnHit(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			if (!((sender as Summon).SummonTypeID != summonTypeID) || !(summonTypeID != ""))
			{
				PlayerController instance = PlayerController.Instance;
				if (!(gameObject == instance) && Random.Range(0f, 1f) < triggerChance)
				{
					RaiseTrigger(gameObject);
				}
			}
		}
	}
}
