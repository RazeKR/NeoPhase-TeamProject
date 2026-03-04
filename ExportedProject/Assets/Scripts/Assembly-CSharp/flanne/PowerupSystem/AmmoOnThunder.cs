using UnityEngine;

namespace flanne.PowerupSystem
{
	public class AmmoOnThunder : MonoBehaviour
	{
		[SerializeField]
		private GameObject staticPrefab;

		[SerializeField]
		private string staticObjectPoolTag;

		[SerializeField]
		private SoundEffectSO sfx;

		[Range(0f, 1f)]
		[SerializeField]
		private float chanceToActivate;

		[SerializeField]
		private int ammoRefillAmount;

		private ObjectPooler OP;

		private Ammo ammo;

		private void Start()
		{
			ammo = base.transform.parent.GetComponentInChildren<Ammo>();
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(staticObjectPoolTag, staticPrefab, 5);
			base.transform.localPosition = Vector3.zero;
			this.AddObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnThunderHit, ThunderGenerator.ThunderHitEvent);
		}

		private void OnThunderHit(object sender, object args)
		{
			if (Random.Range(0f, 1f) < chanceToActivate)
			{
				if (ammo != null)
				{
					ammo.GainAmmo(ammoRefillAmount);
					GameObject pooledObject = OP.GetPooledObject(staticObjectPoolTag);
					pooledObject.transform.SetParent(base.transform);
					pooledObject.transform.localPosition = Vector3.zero;
					pooledObject.SetActive(value: true);
					sfx.Play();
				}
				else
				{
					Debug.LogWarning("No ammo component found");
				}
			}
		}
	}
}
