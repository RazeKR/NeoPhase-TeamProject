using UnityEngine;

namespace flanne
{
	public class ThunderAttackSummon : AttackingSummon
	{
		[SerializeField]
		private int thundersPerAttack;

		[SerializeField]
		private Vector2 range;

		[SerializeField]
		private int baseDamage;

		private ThunderGenerator TGen;

		protected override void Init()
		{
			TGen = ThunderGenerator.SharedInstance;
		}

		protected override bool Attack()
		{
			Vector2 center = base.transform.position;
			GameObject gameObject = null;
			for (int i = 0; i < thundersPerAttack; i++)
			{
				gameObject = EnemyFinder.GetRandomEnemy(center, range);
				if (gameObject != null)
				{
					TGen.GenerateAt(gameObject, Mathf.FloorToInt(base.summonDamageMod.Modify(baseDamage)));
				}
			}
			return gameObject != null;
		}
	}
}
