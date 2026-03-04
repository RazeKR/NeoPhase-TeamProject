using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ApplyThunderAction : Action
	{
		[SerializeField]
		private int damage;

		private ThunderGenerator TG;

		public override void Init()
		{
			TG = ThunderGenerator.SharedInstance;
		}

		public override void Activate(GameObject target)
		{
			TG.GenerateAt(target, damage);
		}
	}
}
