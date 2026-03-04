using UnityEngine;

namespace flanne.PerkSystem.Triggers
{
	public class OnCurseTrigger : Trigger
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToTrigger = 1f;

		public override void OnEquip(PlayerController player)
		{
			this.AddObserver(OnInflictCurse, CurseSystem.InflictCurseEvent);
		}

		public override void OnUnEquip(PlayerController player)
		{
			this.RemoveObserver(OnInflictCurse, CurseSystem.InflictCurseEvent);
		}

		private void OnInflictCurse(object sender, object args)
		{
			GameObject target = args as GameObject;
			if (Random.Range(0f, 1f) < chanceToTrigger)
			{
				RaiseTrigger(target);
			}
		}
	}
}
