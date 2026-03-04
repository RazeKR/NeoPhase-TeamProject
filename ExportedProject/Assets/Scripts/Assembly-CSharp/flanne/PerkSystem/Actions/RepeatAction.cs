using System.Collections;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class RepeatAction : Action
	{
		[SerializeField]
		private int numOfActivations;

		[SerializeField]
		private float delayBetweenActivations;

		[SerializeReference]
		private Action action;

		public override void Init()
		{
			action.Init();
		}

		public override void Activate(GameObject target)
		{
			PlayerController.Instance.StartCoroutine(RepeatActivateCR(target));
		}

		private IEnumerator RepeatActivateCR(GameObject target)
		{
			for (int i = 0; i < numOfActivations; i++)
			{
				action.Activate(target);
				yield return new WaitForSeconds(delayBetweenActivations);
			}
		}
	}
}
