using UnityEngine;

namespace flanne
{
	public class LineController : MonoBehaviour
	{
		[SerializeField]
		private LineRenderer lineRenderer;

		[SerializeField]
		private GameObject[] targets;

		private void Start()
		{
			lineRenderer.positionCount = targets.Length;
			SetLinePositions();
		}

		private void Update()
		{
			SetLinePositions();
		}

		private void SetLinePositions()
		{
			Vector3[] array = new Vector3[targets.Length];
			for (int i = 0; i < targets.Length; i++)
			{
				array[i] = targets[i].transform.position;
			}
			lineRenderer.SetPositions(array);
		}

		public Vector3[] GetPositions()
		{
			Vector3[] array = new Vector3[lineRenderer.positionCount];
			lineRenderer.GetPositions(array);
			return array;
		}

		public float GetWidth()
		{
			return lineRenderer.startWidth;
		}
	}
}
