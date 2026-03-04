using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class BurnSystem : MonoBehaviour
	{
		private class BurnTarget
		{
			public GameObject target;

			public int damage;

			public BurnTarget(GameObject t, int d)
			{
				target = t;
				damage = d;
			}
		}

		public static BurnSystem SharedInstance;

		public static string InflictBurnEvent = "BurnSystem.InflictBurnEvent";

		[SerializeField]
		private GameObject burnFXPrefab;

		[SerializeField]
		private string burnFXOPTag;

		public StatMod burnDamageMultiplier;

		public StatMod burnDurationMultiplier;

		private float baseBurnDuration = 4f;

		private ObjectPooler OP;

		private List<BurnTarget> _currentTargets;

		private float burnDuration => burnDurationMultiplier.Modify(baseBurnDuration) + 0.1f;

		private void Awake()
		{
			SharedInstance = this;
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(burnFXOPTag, burnFXPrefab, 100);
			burnDamageMultiplier = new StatMod();
			burnDurationMultiplier = new StatMod();
			_currentTargets = new List<BurnTarget>();
		}

		public bool IsBurning(GameObject target)
		{
			return _currentTargets.Find((BurnTarget bt) => bt.target == target) != null;
		}

		public void Burn(GameObject target, int burnDamage)
		{
			if (target.tag == "Player")
			{
				return;
			}
			BurnTarget burnTarget = _currentTargets.Find((BurnTarget bt) => bt.target == target);
			if (burnTarget == null)
			{
				Health component = target.GetComponent<Health>();
				if (component != null)
				{
					StartCoroutine(StartBurnCR(component, burnDamage));
				}
				else
				{
					Debug.LogWarning("No health attached to burn target: " + target);
				}
			}
			else
			{
				StartCoroutine(AddBurnCR(burnTarget, burnDamage, burnDuration));
			}
			this.PostNotification(InflictBurnEvent, target);
		}

		private IEnumerator StartBurnCR(Health targetHealth, int burnDamage)
		{
			BurnTarget burnTarget = new BurnTarget(targetHealth.gameObject, 0);
			StartCoroutine(AddBurnCR(burnTarget, burnDamage, burnDuration));
			_currentTargets.Add(burnTarget);
			GameObject burnObj = OP.GetPooledObject(burnFXOPTag);
			burnObj.transform.SetParent(targetHealth.transform);
			burnObj.transform.localPosition = Vector3.zero;
			burnObj.SetActive(value: true);
			yield return null;
			while (targetHealth.gameObject.activeInHierarchy && burnTarget.damage > 0)
			{
				yield return new WaitForSeconds(1f);
				targetHealth.TakeDamage(DamageType.Burn, Mathf.FloorToInt(burnDamageMultiplier.Modify(burnTarget.damage)));
			}
			burnObj.transform.SetParent(OP.transform);
			burnObj.SetActive(value: false);
			_currentTargets.Remove(burnTarget);
		}

		private IEnumerator AddBurnCR(BurnTarget burnTarget, int burnDamage, float duration)
		{
			burnTarget.damage += burnDamage;
			yield return new WaitForSeconds(duration);
			burnTarget.damage -= burnDamage;
		}
	}
}
