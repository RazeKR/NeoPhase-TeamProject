using System.Collections;
using UnityEngine;

namespace flanne
{
	public class SmiteOnHit : MonoBehaviour
	{
		[SerializeField]
		private GameObject smiteFXPrefab;

		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToHit;

		[SerializeField]
		private int baseDamage;

		[SerializeField]
		private SoundEffectSO soundFX;

		private ObjectPooler OP;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(smiteFXPrefab.name, smiteFXPrefab, 50);
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			if (Random.Range(0f, 1f) < chanceToHit)
			{
				GameObject gameObject = args as GameObject;
				if (gameObject.tag.Contains("Enemy"))
				{
					StartCoroutine(SmiteCR(gameObject));
				}
			}
		}

		private IEnumerator SmiteCR(GameObject enemy)
		{
			yield return new WaitForSeconds(0.1f);
			soundFX?.Play();
			GameObject pooledObject = OP.GetPooledObject(smiteFXPrefab.name);
			pooledObject.transform.position = enemy.transform.position + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0f);
			pooledObject.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(-45f, 45f));
			pooledObject.transform.position = enemy.transform.position;
			pooledObject.SetActive(value: true);
			yield return new WaitForSeconds(0.1f);
			enemy.GetComponent<Health>().TakeDamage(DamageType.Smite, baseDamage);
		}
	}
}
