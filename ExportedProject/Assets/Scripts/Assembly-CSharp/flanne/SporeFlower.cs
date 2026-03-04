using UnityEngine;

namespace flanne
{
	public class SporeFlower : AttackingSummon
	{
		[SerializeField]
		private AreaProjectile sporePrefab;

		[SerializeField]
		private Harmful sporeExplosionPrefab;

		[SerializeField]
		private float range = 5f;

		[SerializeField]
		private int baseDamage = 24;

		[SerializeField]
		private Transform firePoint;

		[SerializeField]
		private SoundEffectSO explosionSFX;

		[SerializeField]
		private int sprayThreshold = 5;

		[SerializeField]
		private int sprayAmount = 5;

		[SerializeField]
		private int amountSporeTrack = 5;

		public bool multiplyByBulletDamage;

		public bool procsOnHit;

		private ObjectPooler OP;

		private int _layer;

		private int _hitCtr;

		protected override void Init()
		{
			_layer = 1 << (int)TagLayerUtil.Enemy;
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(sporePrefab.name, sporePrefab.gameObject, 20);
			OP.AddObject(sporeExplosionPrefab.name, sporeExplosionPrefab.gameObject, 20);
		}

		protected override bool Attack()
		{
			Collider2D[] array = new Collider2D[2];
			int num = Physics2D.OverlapCircleNonAlloc(base.transform.position, range, array, _layer);
			if (num == 0)
			{
				return false;
			}
			LaunchSpore(array[Random.Range(0, num)].transform.position, 0.7f);
			return true;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			animator.SetTrigger("Attack");
			_hitCtr++;
			if (_hitCtr >= sprayThreshold)
			{
				_hitCtr -= sprayThreshold;
				SpraySpores();
			}
		}

		private void LaunchSpore(Vector2 targetPos, float duration)
		{
			GameObject pooledObject = OP.GetPooledObject(sporePrefab.name);
			pooledObject.transform.position = firePoint.position;
			pooledObject.SetActive(value: true);
			AreaProjectile spore = pooledObject.GetComponent<AreaProjectile>();
			spore.TargetPos(targetPos, duration);
			spore.onTargetReached.AddListener(delegate
			{
				OnSporeHit(spore);
			});
		}

		private void SpraySpores()
		{
			Collider2D[] array = new Collider2D[amountSporeTrack];
			int num = Physics2D.OverlapCircleNonAlloc(base.transform.position, range, array, _layer);
			for (int i = 0; i < Mathf.Min(sprayAmount, num); i++)
			{
				LaunchSpore(array[i].transform.position, 0.5f + Random.Range(-0.2f, 0.2f));
			}
			for (int j = num; j < sprayAmount; j++)
			{
				Vector2 targetPos = (Vector2)base.transform.position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(1f, range);
				LaunchSpore(targetPos, 0.7f + Random.Range(-0.2f, 0.2f));
			}
		}

		private void OnSporeHit(AreaProjectile spore)
		{
			spore.onTargetReached.RemoveAllListeners();
			GameObject pooledObject = OP.GetPooledObject(sporeExplosionPrefab.name);
			HarmfulOnContact component = pooledObject.GetComponent<HarmfulOnContact>();
			int num = Mathf.CeilToInt(base.summonDamageMod.Modify(baseDamage));
			if (multiplyByBulletDamage)
			{
				num = Mathf.CeilToInt(player.stats[StatType.BulletDamage].Modify(num));
			}
			component.damageAmount = num;
			component.procOnHit = procsOnHit;
			pooledObject.transform.position = spore.transform.position;
			pooledObject.SetActive(value: true);
			explosionSFX?.Play();
		}
	}
}
