using System.Collections.Generic;
using UnityEngine;

namespace flanne.RuneSystem
{
	public class AttachObjectRune : Rune
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int amountToAttach = 1;

		[SerializeField]
		private Vector3 posOffset;

		protected override void Init()
		{
			for (int i = 0; i < amountToAttach; i++)
			{
				GameObject obj = Object.Instantiate(prefab);
				obj.transform.SetParent(player.transform);
				obj.transform.localPosition = posOffset;
				Orbital component = obj.GetComponent<Orbital>();
				if (!(component != null))
				{
					continue;
				}
				Orbital[] componentsInChildren = player.GetComponentsInChildren<Orbital>();
				List<Orbital> list = new List<Orbital>();
				Orbital[] array = componentsInChildren;
				foreach (Orbital orbital in array)
				{
					if (orbital.tag == component.tag)
					{
						list.Add(orbital);
					}
				}
				Vector2 v = list[0].transform.localPosition;
				for (int k = 1; k < list.Count; k++)
				{
					list[k].transform.localPosition = v.Rotate(k * (360 / list.Count));
				}
			}
		}
	}
}
