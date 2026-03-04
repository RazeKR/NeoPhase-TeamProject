using System.Collections;
using UnityEngine;

namespace flanne
{
	public class StabbingSummon : AttackingSummon
	{
		[SerializeField]
		private int baseDamage = 50;

		[SerializeField]
		private float range;

		private IEnumerator _stabbingCoroutine;

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains("Enemy"))
			{
				Health component = other.gameObject.GetComponent<Health>();
				if (!(component == null))
				{
					int damage = Mathf.FloorToInt(base.summonDamageMod.Modify(baseDamage));
					damage = ApplyDamageMods(damage);
					component.TakeDamage(DamageType.Summon, damage);
					this.PostNotification(Summon.SummonOnHitNotification, other.gameObject);
				}
			}
		}

		protected override bool Attack()
		{
			if (_stabbingCoroutine != null)
			{
				return false;
			}
			Vector2 vector = base.transform.position;
			if (EnemyFinder.GetRandomEnemy(vector, new Vector2(9f, 6f)) == null)
			{
				return false;
			}
			Vector2 vector2 = (Vector2)EnemyFinder.GetRandomEnemy(vector, new Vector2(9f, 6f)).transform.position - vector;
			if (vector2.magnitude > range)
			{
				return false;
			}
			Vector2 destination = vector2.normalized * range + vector;
			_stabbingCoroutine = StabCR(destination);
			StartCoroutine(_stabbingCoroutine);
			return true;
		}

		private IEnumerator StabCR(Vector2 destination)
		{
			Vector2 vector = base.transform.position;
			Vector2 vector2 = destination - vector;
			Quaternion originalRotation = base.transform.localRotation;
			float angle = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
			base.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			Transform parent = base.transform.parent;
			base.transform.SetParent(null);
			int tweenID = LeanTween.move(base.gameObject, destination, base.finalAttackCooldown / 2f).id;
			while (LeanTween.isTweening(tweenID))
			{
				yield return null;
			}
			base.transform.SetParent(parent);
			tweenID = LeanTween.moveLocal(base.gameObject, Vector3.zero, base.finalAttackCooldown / 2f).id;
			while (LeanTween.isTweening(tweenID))
			{
				yield return null;
			}
			base.transform.localRotation = originalRotation;
			_stabbingCoroutine = null;
		}
	}
}
