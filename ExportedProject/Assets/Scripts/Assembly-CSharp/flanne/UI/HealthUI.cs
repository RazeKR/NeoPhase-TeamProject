using System.Collections.Generic;
using UnityEngine;

namespace flanne.UI
{
	public class HealthUI : MonoBehaviour
	{
		[SerializeField]
		private PlayerHealth playerHealth;

		[SerializeField]
		private GameObject heartPrefab;

		[SerializeField]
		private GameObject emptyHeartPrefab;

		[SerializeField]
		private GameObject soulHeartPrefab;

		private List<GameObject> hearts;

		private int hp;

		private int mhp;

		private int shp;

		private void Start()
		{
			playerHealth.onHealthChangedTo.AddListener(OnHpChanged);
			playerHealth.onMaxHPChangedTo.AddListener(OnMaxHPChanged);
			playerHealth.onSoulHPChangedTo.AddListener(OnSoulHpChanged);
		}

		private void OnDestroy()
		{
			playerHealth.onHealthChangedTo.RemoveListener(OnHpChanged);
			playerHealth.onMaxHPChangedTo.RemoveListener(OnMaxHPChanged);
			playerHealth.onSoulHPChangedTo.RemoveListener(OnSoulHpChanged);
		}

		public void OnHpChanged(int value)
		{
			hp = value;
			Refresh();
		}

		public void OnMaxHPChanged(int value)
		{
			mhp = value;
			Refresh();
		}

		public void OnSoulHpChanged(int value)
		{
			shp = value;
			Refresh();
		}

		private void Refresh()
		{
			if (hearts == null)
			{
				hearts = new List<GameObject>();
			}
			foreach (GameObject heart in hearts)
			{
				Object.Destroy(heart);
			}
			hearts.Clear();
			int i;
			for (i = 0; i < hp; i++)
			{
				GameObject gameObject = Object.Instantiate(heartPrefab);
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				hearts.Add(gameObject);
			}
			for (; i < mhp; i++)
			{
				GameObject gameObject2 = Object.Instantiate(emptyHeartPrefab);
				gameObject2.transform.SetParent(base.transform);
				gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
				hearts.Add(gameObject2);
			}
			for (int j = 0; j < shp; j++)
			{
				GameObject gameObject3 = Object.Instantiate(soulHeartPrefab);
				gameObject3.transform.SetParent(base.transform);
				gameObject3.transform.localScale = new Vector3(1f, 1f, 1f);
				hearts.Add(gameObject3);
			}
		}
	}
}
