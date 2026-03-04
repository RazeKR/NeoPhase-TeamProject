using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class FreezeSystem : MonoBehaviour
	{
		private class FreezeTarget
		{
			public GameObject target;

			public float duration;

			public FreezeTarget(GameObject t, float d)
			{
				target = t;
				duration = d;
			}
		}

		public static FreezeSystem SharedInstance;

		public static string InflictFreezeEvent = "FreezeSystem.InflictFreezeEvent";

		public StatMod durationMod;

		[SerializeField]
		private float freezeDuration = 1.5f;

		[SerializeField]
		private GameObject freezeFXPrefab;

		[SerializeField]
		private GameObject freezeFXLargePrefab;

		[SerializeField]
		private SoundEffectSO soundFX;

		private ObjectPooler OP;

		private List<FreezeTarget> _currentTargets;

		private void Awake()
		{
			SharedInstance = this;
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(freezeFXPrefab.name, freezeFXPrefab, 100);
			OP.AddObject(freezeFXLargePrefab.name, freezeFXLargePrefab, 100);
			_currentTargets = new List<FreezeTarget>();
			durationMod = new StatMod();
		}

		public bool IsFrozen(GameObject target)
		{
			return _currentTargets.Find((FreezeTarget bt) => bt.target == target) != null;
		}

		public void Freeze(GameObject target)
		{
			if ((bool)target.GetComponent<AIComponent>())
			{
				FreezeTarget freezeTarget = _currentTargets.Find((FreezeTarget bt) => bt.target == target);
				float num = ((!target.tag.Contains("Champion")) ? freezeDuration : (freezeDuration / 10f));
				if (freezeTarget == null)
				{
					freezeTarget = new FreezeTarget(target, durationMod.Modify(num));
					StartCoroutine(StartFreezeCR(freezeTarget));
				}
				else if (num > freezeTarget.duration)
				{
					freezeTarget.duration = durationMod.Modify(num);
				}
				soundFX?.Play();
			}
		}

		private IEnumerator StartFreezeCR(FreezeTarget freezeTarget)
		{
			this.PostNotification(InflictFreezeEvent, freezeTarget.target);
			AIComponent component = freezeTarget.target.GetComponent<AIComponent>();
			if (component != null)
			{
				component.frozen = true;
			}
			Animator animator = freezeTarget.target.GetComponent<Animator>();
			if (animator != null)
			{
				animator.speed = 0f;
			}
			_currentTargets.Add(freezeTarget);
			string text = ((!freezeTarget.target.tag.Contains("Champion")) ? freezeFXPrefab.name : freezeFXLargePrefab.name);
			GameObject freezeObj = OP.GetPooledObject(text);
			freezeObj.transform.SetParent(freezeTarget.target.transform);
			freezeObj.transform.localPosition = Vector3.zero;
			freezeObj.SetActive(value: true);
			while (freezeTarget.duration > 0f && freezeTarget.target.activeInHierarchy)
			{
				yield return null;
				freezeTarget.duration -= Time.deltaTime;
			}
			freezeTarget.target.GetComponent<AIComponent>().frozen = false;
			if (animator != null)
			{
				animator.speed = 1f;
			}
			_currentTargets.Remove(freezeTarget);
			freezeObj.transform.SetParent(OP.transform);
			freezeObj.SetActive(value: false);
		}
	}
}
