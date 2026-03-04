using System.Collections;
using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AIRandomAreaProjectileSpecial", menuName = "AISpecials/AIRandomAreaProjectileSpecial")]
	public class AIRandomAreaProjectileSpecial : AISpecial
	{
		[SerializeField]
		private EnemyAreaProjectile projectilePrefab;

		[SerializeField]
		private float windupTime = 0.2f;

		[SerializeField]
		private int numProjectiles = 1;

		[SerializeField]
		private Vector2 range = new Vector2(50f, 50f);

		[SerializeField]
		private Vector2 airTime = new Vector2(0f, 2f);

		public override void Use(AIComponent ai, Transform target)
		{
			ai.StartCoroutine(ShootCR(ai.specialPoint.transform, target, ai));
		}

		private void InitProjectile(Vector2 spawnPos, Vector2 hitPos, float duration)
		{
			EnemyAreaProjectile component = Object.Instantiate(projectilePrefab.gameObject).GetComponent<EnemyAreaProjectile>();
			component.transform.position = spawnPos;
			component.TargetPos(hitPos, duration);
		}

		private IEnumerator ShootCR(Transform spawn, Transform target, AIComponent ai)
		{
			ai.animator?.SetTrigger("Windup");
			yield return new WaitForSeconds(windupTime);
			ai.animator?.SetTrigger("Special");
			Vector3 vector = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f);
			InitProjectile(spawn.position, target.position + vector, 1f);
			for (int i = 0; i < numProjectiles; i++)
			{
				Vector2 normalized = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
				Vector2 hitPos = (Vector2)ai.transform.position + normalized * Random.Range(range.x, range.y);
				float duration = Random.Range(airTime.x, airTime.y);
				InitProjectile(spawn.transform.position, hitPos, duration);
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
}
