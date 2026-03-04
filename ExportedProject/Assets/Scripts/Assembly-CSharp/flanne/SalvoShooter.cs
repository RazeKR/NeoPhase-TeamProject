using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class SalvoShooter : Shooter
	{
		[SerializeField]
		private GameObject targetIndicatorPrefab;

		[SerializeField]
		private float distanceToLockOn;

		[SerializeField]
		private float range = 7f;

		[SerializeField]
		private SoundEffectSO targetAcquiredSFX;

		private PlayerController player;

		private List<GameObject> _indicators = new List<GameObject>();

		private List<Transform> _targets = new List<Transform>();

		private int _layer;

		private float _distanceCtr;

		private Vector2 _lastPos;

		private Vector2 _currPos;

		private IEnumerator _shootSalveCR;

		public override void Init()
		{
			_layer = 1 << (int)TagLayerUtil.Enemy;
			player = PlayerController.Instance;
			_lastPos = player.transform.position;
			_currPos = player.transform.position;
			OP.AddObject(targetIndicatorPrefab.name, targetIndicatorPrefab, 20);
		}

		private void Update()
		{
			if (_shootSalveCR != null)
			{
				return;
			}
			UpdateDistanceMoved();
			UpdateIndicators();
			if (_distanceCtr >= distanceToLockOn)
			{
				_distanceCtr -= distanceToLockOn;
				Transform closestUnlockedEnemy = GetClosestUnlockedEnemy();
				if (closestUnlockedEnemy != null)
				{
					LockOn(closestUnlockedEnemy);
				}
			}
		}

		private void OnDisable()
		{
			foreach (GameObject indicator in _indicators)
			{
				indicator.SetActive(value: false);
			}
			_indicators.Clear();
		}

		public override void Shoot(ProjectileRecipe recipe, Vector2 pointDirection, int numProjectiles, float spread, float inaccuracy)
		{
			if (_shootSalveCR == null)
			{
				_shootSalveCR = SalvoShootCR(recipe, numProjectiles, spread, inaccuracy);
				StartCoroutine(_shootSalveCR);
			}
		}

		private void UpdateDistanceMoved()
		{
			_lastPos = _currPos;
			_currPos = player.transform.position;
			_distanceCtr += (_lastPos - _currPos).magnitude;
		}

		private void UpdateIndicators()
		{
			for (int num = _targets.Count - 1; num >= 0; num--)
			{
				if (_targets[num].gameObject.activeSelf)
				{
					_indicators[num].transform.position = _targets[num].position;
				}
				else
				{
					_indicators[num].SetActive(value: false);
					_indicators.RemoveAt(num);
					_targets.RemoveAt(num);
				}
			}
		}

		private Transform GetClosestUnlockedEnemy()
		{
			Collider2D[] array = new Collider2D[250];
			int num = Physics2D.OverlapCircleNonAlloc(base.transform.position, range, array, _layer);
			Transform result = null;
			float num2 = float.PositiveInfinity;
			Vector2 vector = base.transform.position;
			for (int i = 0; i < num; i++)
			{
				Transform transform = array[i].transform;
				if (!_targets.Contains(transform) && !transform.tag.Contains("Passive"))
				{
					float magnitude = ((Vector2)transform.position - vector).magnitude;
					if (magnitude < num2)
					{
						num2 = magnitude;
						result = transform;
					}
				}
			}
			return result;
		}

		private void LockOn(Transform target)
		{
			if (_targets.Count < player.ammo.amount)
			{
				_targets.Add(target);
				GameObject pooledObject = OP.GetPooledObject(targetIndicatorPrefab.name);
				_indicators.Add(pooledObject);
				pooledObject.transform.position = target.position;
				pooledObject.SetActive(value: true);
				targetAcquiredSFX?.Play();
			}
		}

		private IEnumerator SalvoShootCR(ProjectileRecipe recipe, int numProjectiles, float spread, float inaccuracy)
		{
			foreach (GameObject indicator in _indicators)
			{
				indicator.SetActive(value: false);
			}
			_indicators.Clear();
			float delayBetweenShots = player.stats[StatType.FireRate].ModifyInverse(0.05f);
			foreach (Transform target in _targets)
			{
				if (player.ammo.amount != 0)
				{
					player.gun.gunData.gunshotSFX.Play();
					Vector3 vector = target.position - base.transform.position;
					base.Shoot(recipe, vector, numProjectiles, spread, inaccuracy);
					yield return new WaitForSeconds(delayBetweenShots);
				}
			}
			_targets.Clear();
			_shootSalveCR = null;
		}
	}
}
