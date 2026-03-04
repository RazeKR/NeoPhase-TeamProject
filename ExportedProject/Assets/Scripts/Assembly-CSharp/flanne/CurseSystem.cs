using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class CurseSystem : MonoBehaviour
	{
		public static CurseSystem Instance;

		public static string InflictCurseEvent = "CurseSystem.InflictCurseEvent";

		public static string CurseKillNotification = "CurseSystem.CurseKillNotification";

		public float duration = 0.7f;

		public float curseDamageMultiplier = 2f;

		[SerializeField]
		private GameObject curseFXPrefab;

		private ObjectPooler OP;

		private Gun myGun;

		private List<Health> _cursedTargets;

		public int curseDamage => Mathf.FloorToInt(myGun.damage * curseDamageMultiplier);

		private void Awake()
		{
			if (Instance != null)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				Instance = this;
			}
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(curseFXPrefab.name, curseFXPrefab, 100);
			_cursedTargets = new List<Health>();
			myGun = PlayerController.Instance.gun;
		}

		public bool IsCursed(GameObject target)
		{
			return _cursedTargets.Find((Health bt) => bt.gameObject == target) != null;
		}

		public void Curse(GameObject target)
		{
			Health component = target.GetComponent<Health>();
			if (!(component == null) && !_cursedTargets.Contains(component))
			{
				StartCoroutine(CurseCR(component));
				this.PostNotification(InflictCurseEvent, target);
			}
		}

		private IEnumerator CurseCR(Health targetHealth)
		{
			_cursedTargets.Add(targetHealth);
			GameObject curseObj = OP.GetPooledObject(curseFXPrefab.name);
			curseObj.transform.SetParent(targetHealth.transform);
			curseObj.transform.localPosition = Vector3.zero;
			curseObj.SetActive(value: true);
			yield return new WaitForSeconds(duration);
			_cursedTargets.Remove(targetHealth);
			if (targetHealth.gameObject.activeSelf)
			{
				targetHealth.TakeDamage(DamageType.Curse, curseDamage);
			}
			curseObj.SetActive(value: false);
			curseObj.transform.SetParent(OP.transform);
			curseObj.SetActive(value: false);
		}
	}
}
