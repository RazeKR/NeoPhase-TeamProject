using UnityEngine;

namespace flanne.PowerupSystem
{
	public class ThunderOnAttack : AttackOnShoot
	{
		[SerializeField]
		private int baseDamage;

		[SerializeField]
		private int numThunderPerAttack = 1;

		private ThunderGenerator TGen;

		private ShootingCursor SC;

		protected override void Init()
		{
			TGen = ThunderGenerator.SharedInstance;
			SC = ShootingCursor.Instance;
		}

		public override void Attack()
		{
			for (int i = 0; i < numThunderPerAttack; i++)
			{
				GameObject closestEnemy = EnemyFinder.GetClosestEnemy(Camera.main.ScreenToWorldPoint(SC.cursorPosition));
				if (closestEnemy != null)
				{
					TGen.GenerateAt(closestEnemy, baseDamage);
				}
			}
		}
	}
}
