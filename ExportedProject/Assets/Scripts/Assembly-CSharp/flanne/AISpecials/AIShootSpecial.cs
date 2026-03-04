using System.Collections;
using UnityEngine;

namespace flanne.AISpecials
{
	[CreateAssetMenu(fileName = "AIShootSpecial", menuName = "AISpecials/AIShootSpecial")]
	public class AIShootSpecial : AISpecial
	{
		[SerializeField]
		private string projectileOPTag;

		[SerializeField]
		private float projectileSpeed;

		[SerializeField]
		private int numRepeated = 1;

		[SerializeField]
		private float delayBetweenShots;

		[SerializeField]
		private SoundEffectSO soundFX;

		public override void Use(AIComponent ai, Transform target)
		{
			Vector3 direction = target.position - ai.specialPoint.position;
			ai.StartCoroutine(ShootCR(direction, ai.specialPoint.transform));
		}

		private IEnumerator ShootCR(Vector3 direction, Transform spawn)
		{
			for (int i = 0; i < numRepeated; i++)
			{
				GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject(projectileOPTag);
				pooledObject.SetActive(value: true);
				pooledObject.transform.position = spawn.position;
				pooledObject.GetComponent<MoveComponent2D>().vector = projectileSpeed * direction.normalized;
				soundFX?.Play();
				yield return new WaitForSeconds(delayBetweenShots);
			}
		}
	}
}
