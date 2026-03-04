using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class AttachPrefabToPlayerOnStart : MonoBehaviour
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private Vector3 posOffset;

		private void Start()
		{
			PlayerController instance = PlayerController.Instance;
			GameObject obj = Object.Instantiate(prefab);
			obj.transform.SetParent(instance.transform);
			obj.transform.localPosition = posOffset;
			Orbital component = obj.GetComponent<Orbital>();
			if (!(component != null))
			{
				return;
			}
			component.center = instance.transform;
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
