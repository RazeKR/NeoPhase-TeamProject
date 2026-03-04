using UnityEngine;

namespace flanne
{
	public class ThunderGenerator : MonoBehaviour
	{
		public static ThunderGenerator SharedInstance;

		public static string ThunderHitEvent = "ThunderGenerator.ThunderHitEvent";

		[SerializeField]
		private GameObject thunderPrefab;

		[SerializeField]
		private GameObject thunderImpactPrefab;

		[SerializeField]
		private SoundEffectSO soundFX;

		[SerializeField]
		private float baseAoE = 0.35f;

		public StatMod damageMod;

		public float sizeMultiplier;

		private ObjectPooler OP;

		private void Awake()
		{
			SharedInstance = this;
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(thunderPrefab.name, thunderPrefab, 100);
			OP.AddObject(thunderImpactPrefab.name, thunderImpactPrefab, 100);
			sizeMultiplier = 1f;
			damageMod = new StatMod();
		}

		public void GenerateAt(GameObject target, int damage)
		{
			GenerateAt(target.transform.position, damage);
			this.PostNotification(ThunderHitEvent, target);
		}

		public void GenerateAt(Vector3 position, int damage)
		{
			Vector3 position2 = position + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0f);
			GameObject pooledObject = OP.GetPooledObject(thunderPrefab.name);
			pooledObject.transform.position = position2;
			pooledObject.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(-45f, 45f));
			pooledObject.SetActive(value: true);
			GameObject pooledObject2 = OP.GetPooledObject(thunderImpactPrefab.name);
			pooledObject2.transform.localScale = Vector3.one * sizeMultiplier;
			pooledObject2.transform.position = position2;
			pooledObject2.SetActive(value: true);
			Collider2D[] array = Physics2D.OverlapCircleAll(position, baseAoE * sizeMultiplier, 1 << (int)TagLayerUtil.Enemy);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].GetComponent<Health>()?.TakeDamage(DamageType.Thunder, Mathf.FloorToInt(damageMod.Modify(damage)));
			}
			pooledObject.GetComponent<SpriteRenderer>().flipX = Random.Range(0, 1) == 0;
			soundFX.Play();
		}
	}
}
