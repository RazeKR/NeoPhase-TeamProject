using System.Collections;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class DelayActivateAction : Action
	{
		[SerializeField]
		private float delayTime;

		[SerializeReference]
		private Action action;

		public override void Init()
		{
			action.Init();
		}

		public override void Activate(GameObject target)
		{
			PlayerController.Instance.StartCoroutine(DelayActivateCR(target));
		}

		private IEnumerator DelayActivateCR(GameObject target)
		{
			yield return new WaitForSeconds(delayTime);
			action.Activate(target);
		}
	}
}
