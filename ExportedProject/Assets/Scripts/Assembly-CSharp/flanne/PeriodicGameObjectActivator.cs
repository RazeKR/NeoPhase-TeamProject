using UnityEngine;

namespace flanne
{
	public class PeriodicGameObjectActivator : MonoBehaviour
	{
		[SerializeField]
		private GameObject obj;

		public float timeBetweenActivations;

		private void Start()
		{
			InvokeRepeating("ActivateObject", timeBetweenActivations, timeBetweenActivations);
		}

		private void ActivateObject()
		{
			obj.SetActive(value: true);
		}
	}
}
