using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class ArrowRetrieveOnReload : MonoBehaviour
	{
		private static List<ArrowRetrievalPoint> retrievalPoints;

		[SerializeField]
		private GameObject retrieveArrowPrefab;

		[SerializeField]
		private float retrieveSpeed;

		[SerializeField]
		private float damageMulti = 1f;

		[SerializeField]
		private bool dontRetrieve;

		[SerializeField]
		private SoundEffectSO retrieveSFX;

		private ObjectPooler OP;

		private PlayerController player;

		private Gun gun;

		private Ammo ammo;

		private void OnReload()
		{
			for (int i = 0; i < retrievalPoints.Count; i++)
			{
				if (retrievalPoints[i].gameObject.activeSelf)
				{
					GameObject pooledObject = OP.GetPooledObject(retrieveArrowPrefab.name);
					pooledObject.transform.position = retrievalPoints[i].transform.position;
					pooledObject.SetActive(value: true);
					Harmful component = pooledObject.GetComponent<Harmful>();
					component.damageAmount = Mathf.FloorToInt(gun.damage * damageMulti);
					if (!dontRetrieve)
					{
						component.StartCoroutine(RetrieveCR(pooledObject));
					}
					retrievalPoints[i].gameObject.SetActive(value: false);
				}
			}
			retrieveSFX?.Play();
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(retrieveArrowPrefab.name, retrieveArrowPrefab, 100);
			player = GetComponentInParent<PlayerController>();
			gun = player.gun;
			ammo = player.ammo;
			ammo.OnReload.AddListener(OnReload);
		}

		private void OnDestroy()
		{
			ammo.OnReload.RemoveListener(OnReload);
		}

		public static void RegisterRetrievalPoint(ArrowRetrievalPoint r)
		{
			if (retrievalPoints == null)
			{
				retrievalPoints = new List<ArrowRetrievalPoint>();
			}
			retrievalPoints.Add(r);
		}

		public static void RemoveRetrievalPoint(ArrowRetrievalPoint r)
		{
			retrievalPoints.Remove(r);
		}

		private IEnumerator RetrieveCR(GameObject obj)
		{
			obj.transform.SetParent(player.transform);
			while (obj.transform.localPosition != Vector3.zero)
			{
				yield return null;
				float maxDistanceDelta = retrieveSpeed * Time.deltaTime;
				Vector3 localPosition = Vector3.MoveTowards(obj.transform.localPosition, Vector3.zero, maxDistanceDelta);
				obj.transform.localPosition = localPosition;
			}
			obj.transform.SetParent(ObjectPooler.SharedInstance.transform);
			obj.transform.localPosition = Vector3.zero;
			obj.SetActive(value: false);
		}
	}
}
