using UnityEngine;

namespace flanne
{
	public class SetHitboxActiveSummon : AttackingSummon
	{
		public int baseDamage;

		[SerializeField]
		private HarmfulOnContact hitbox;

		[SerializeField]
		private float range;

		private void OnHit(object sender, object args)
		{
			GameObject e = args as GameObject;
			this.PostNotification(Summon.SummonOnHitNotification, e);
		}

		protected override void Init()
		{
			this.AddObserver(OnHit, HarmfulOnContact.HitNotification, hitbox);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnHit, HarmfulOnContact.HitNotification, hitbox);
		}

		protected override bool Attack()
		{
			Vector2 vector = base.transform.position;
			GameObject randomEnemy = EnemyFinder.GetRandomEnemy(vector, new Vector2(range, range));
			if (randomEnemy != null)
			{
				hitbox.GetComponent<HarmfulOnContact>().damageAmount = Mathf.FloorToInt(base.summonDamageMod.Modify(baseDamage));
				Vector2 vector2 = (Vector2)randomEnemy.transform.position - vector;
				float angle = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
				hitbox.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
				hitbox.gameObject.SetActive(value: true);
				return true;
			}
			return false;
		}
	}
}
