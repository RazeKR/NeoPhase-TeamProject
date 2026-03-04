using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class ThunderNearCursorAction : Action
	{
		[SerializeField]
		private int baseDamage;

		public override void Activate(GameObject target)
		{
			GameObject closestEnemy = EnemyFinder.GetClosestEnemy(Camera.main.ScreenToWorldPoint(ShootingCursor.Instance.cursorPosition));
			if (closestEnemy != null)
			{
				ThunderGenerator.SharedInstance.GenerateAt(closestEnemy, baseDamage);
			}
		}
	}
}
