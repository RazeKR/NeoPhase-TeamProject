using TMPro;
using UnityEngine;

namespace flanne
{
	public class DamagePopupSpawner : MonoBehaviour
	{
		[SerializeField]
		private string popupOpTag;

		public void OnDamageTaken(int amount)
		{
			if (amount != 0)
			{
				GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject(popupOpTag);
				pooledObject.transform.position = base.transform.position;
				pooledObject.SetActive(value: true);
				pooledObject.GetComponent<TextMeshPro>().text = Mathf.Abs(amount).ToString();
			}
		}
	}
}
