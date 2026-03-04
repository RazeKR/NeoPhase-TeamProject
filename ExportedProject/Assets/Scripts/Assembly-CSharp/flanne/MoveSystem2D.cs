using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class MoveSystem2D : MonoBehaviour
	{
		public static List<MoveComponent2D> moveComponents;

		private ObjectPooler OP;

		private void Awake()
		{
			moveComponents = new List<MoveComponent2D>();
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
		}

		private void FixedUpdate()
		{
			for (int i = 0; i < moveComponents.Count; i++)
			{
				if (!moveComponents[i].gameObject.activeSelf)
				{
					continue;
				}
				float magnitude = moveComponents[i].vector.magnitude;
				magnitude *= Mathf.Exp((0f - moveComponents[i].drag) * Time.fixedDeltaTime);
				moveComponents[i].vector = Mathf.Max(0f, magnitude) * moveComponents[i].vector.normalized;
				moveComponents[i].Rb.MovePosition(moveComponents[i].Rb.position + moveComponents[i].vector * Time.fixedDeltaTime);
				if (moveComponents[i].rotateTowardsMove)
				{
					Quaternion rotation = Quaternion.AngleAxis(Mathf.Atan2(moveComponents[i].vector.y, moveComponents[i].vector.x) * 57.29578f, Vector3.forward);
					moveComponents[i].transform.rotation = rotation;
					if (moveComponents[i].vector.x < 0f)
					{
						moveComponents[i].transform.localScale = new Vector3(1f, -1f, 1f);
					}
					else
					{
						moveComponents[i].transform.localScale = new Vector3(1f, 1f, 1f);
					}
				}
			}
		}

		public static void Register(MoveComponent2D m)
		{
			moveComponents.Add(m);
		}

		public static void UnRegister(MoveComponent2D m)
		{
			moveComponents.Remove(m);
		}
	}
}
