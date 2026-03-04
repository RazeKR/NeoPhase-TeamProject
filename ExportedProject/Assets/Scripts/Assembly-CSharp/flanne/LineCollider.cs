using System;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class LineCollider : MonoBehaviour
	{
		[SerializeField]
		private LineController lineController;

		[SerializeField]
		private PolygonCollider2D polygonCollider2D;

		[SerializeField]
		private bool loop;

		private List<Vector2> colliderPoints = new List<Vector2>();

		private void LateUpdate()
		{
			Vector3[] positions = lineController.GetPositions();
			if (positions.Length >= 2)
			{
				int num = positions.Length - 1;
				if (loop)
				{
					polygonCollider2D.pathCount = num + 1;
				}
				else
				{
					polygonCollider2D.pathCount = num;
				}
				for (int i = 0; i < num; i++)
				{
					List<Vector2> positions2 = new List<Vector2>
					{
						positions[i],
						positions[i + 1]
					};
					List<Vector2> list = CalculateColliderPoints(positions2);
					polygonCollider2D.SetPath(i, list.ConvertAll((Converter<Vector2, Vector2>)((Vector2 p) => base.transform.InverseTransformPoint(p))));
				}
				if (loop)
				{
					List<Vector2> positions3 = new List<Vector2>
					{
						positions[num],
						positions[0]
					};
					List<Vector2> list2 = CalculateColliderPoints(positions3);
					polygonCollider2D.SetPath(num, list2.ConvertAll((Converter<Vector2, Vector2>)((Vector2 p) => base.transform.InverseTransformPoint(p))));
				}
			}
			else
			{
				polygonCollider2D.pathCount = 0;
			}
		}

		private List<Vector2> CalculateColliderPoints(List<Vector2> positions)
		{
			float width = lineController.GetWidth();
			float num = (positions[1].y - positions[0].y) / (positions[1].x - positions[0].x);
			float num2 = width / 2f * (num / Mathf.Pow(num * num + 1f, 0.5f));
			float num3 = width / 2f * (1f / Mathf.Pow(1f + num * num, 0.5f));
			Vector2[] array = new Vector2[2]
			{
				new Vector2(0f - num2, num3),
				new Vector2(num2, 0f - num3)
			};
			return new List<Vector2>
			{
				positions[0] + array[0],
				positions[1] + array[0],
				positions[1] + array[1],
				positions[0] + array[1]
			};
		}
	}
}
