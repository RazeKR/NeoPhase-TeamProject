using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class AddShooterAction : Action
	{
		[SerializeField]
		private GameObject prefab;

		public override void Activate(GameObject target)
		{
			Gun componentInChildren = target.GetComponentInChildren<Gun>();
			GameObject gameObject = Object.Instantiate(prefab);
			gameObject.transform.SetParent(componentInChildren.transform);
			gameObject.transform.localPosition = Vector2.zero;
			Shooter componentInChildren2 = gameObject.GetComponentInChildren<Shooter>();
			componentInChildren.AddShooter(componentInChildren2);
		}
	}
}
