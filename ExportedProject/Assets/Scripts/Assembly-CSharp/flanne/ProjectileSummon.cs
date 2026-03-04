using System.Collections;
using UnityEngine;

namespace flanne
{
	public class ProjectileSummon : Summon
	{
		[SerializeField]
		private int baseDamage = 1;

		protected override void Init()
		{
			SeekEnemy component = GetComponent<SeekEnemy>();
			if (component != null)
			{
				component.player = player.transform;
			}
		}

		private void OnEnable()
		{
			StartCoroutine(WaitToUnParentCR());
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag == "Enemy")
			{
				Health component = other.gameObject.GetComponent<Health>();
				if (component != null)
				{
					component.HPChange(Mathf.FloorToInt(-1f * base.summonDamageMod.Modify(baseDamage)));
				}
				base.gameObject.SetActive(value: false);
			}
		}

		private IEnumerator WaitToUnParentCR()
		{
			yield return null;
			base.transform.SetParent(null);
		}
	}
}
