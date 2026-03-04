using UnityEngine;

namespace flanne
{
	public class SpawnOnCollision : MonoBehaviour
	{
		[SerializeField]
		private string hitTag;

		[SerializeField]
		private string objPoolTag;

		private GameObject lastImpactFX;

		private ObjectPooler OP;

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(hitTag))
			{
				for (int i = 0; i < other.contacts.Length; i++)
				{
					lastImpactFX = OP.GetPooledObject(objPoolTag);
					lastImpactFX.SetActive(value: true);
					lastImpactFX.transform.position = other.contacts[i].point;
				}
			}
		}
	}
}
