using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class AreaTargetAction : Action
	{
		[SerializeField]
		private float range;

		[SerializeReference]
		private Action action;

		public override void Init()
		{
			action.Init();
		}

		public override void Activate(GameObject target)
		{
			Collider2D[] array = Physics2D.OverlapCircleAll(target.transform.position, range, 1 << (int)TagLayerUtil.Enemy);
			foreach (Collider2D collider2D in array)
			{
				action.Activate(collider2D.gameObject);
			}
		}
	}
}
