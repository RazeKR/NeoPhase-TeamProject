using System.Collections.Generic;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class AttachPrefabToPlayerAction : Action
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int amountToAttach = 1;

		[SerializeField]
		private Vector3 posOffset;

		public override void Activate(GameObject target)
		{
			for (int i = 0; i < amountToAttach; i++)
			{
				PowerupReference componentInChildren = prefab.GetComponentInChildren<PowerupReference>();
				if (componentInChildren == null)
				{
					AttachPrefab(prefab);
					continue;
				}
				PowerupReference[] componentsInChildren = target.GetComponentsInChildren<PowerupReference>();
				bool flag = false;
				PowerupReference[] array = componentsInChildren;
				foreach (PowerupReference powerupReference in array)
				{
					if (powerupReference.powerup == componentInChildren.powerup)
					{
						AttachPrefab(powerupReference.gameObject);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					AttachPrefab(prefab);
				}
			}
		}

		private void AttachPrefab(GameObject prefab)
		{
			PlayerController instance = PlayerController.Instance;
			GameObject gameObject = Object.Instantiate(prefab);
			gameObject.transform.SetParent(instance.transform);
			gameObject.transform.localPosition = posOffset;
			Orbital component = gameObject.GetComponent<Orbital>();
			if (!(component != null))
			{
				return;
			}
			Orbital[] componentsInChildren = instance.GetComponentsInChildren<Orbital>();
			List<Orbital> list = new List<Orbital>();
			Orbital[] array = componentsInChildren;
			foreach (Orbital orbital in array)
			{
				if (orbital.tag == component.tag)
				{
					list.Add(orbital);
				}
			}
			Vector2 v = posOffset;
			for (int j = 0; j < list.Count; j++)
			{
				int num = j * (360 / list.Count);
				list[j].transform.localPosition = v.Rotate(num);
				if (!component.dontRotate)
				{
					list[j].transform.rotation = Quaternion.Euler(0f, 0f, num);
				}
			}
		}
	}
}
