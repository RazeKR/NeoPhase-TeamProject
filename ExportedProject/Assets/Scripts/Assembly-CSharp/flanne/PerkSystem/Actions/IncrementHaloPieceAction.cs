using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class IncrementHaloPieceAction : Action
	{
		[SerializeField]
		private GameObject haloManagerPrefab;

		public override void Activate(GameObject target)
		{
			ShanasHalo componentInChildren = target.GetComponentInChildren<ShanasHalo>();
			if (componentInChildren == null)
			{
				GameObject gameObject = Object.Instantiate(haloManagerPrefab);
				gameObject.transform.SetParent(target.transform);
				gameObject.transform.localPosition = Vector3.zero;
			}
			else
			{
				componentInChildren.CollectPiece();
			}
		}
	}
}
