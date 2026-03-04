using System.Collections;
using UnityEngine;

namespace flanne
{
	public class SmitePassive : MonoBehaviour
	{
		public static string SmiteTweakDamageNotification = "SmitePassive.SmiteTweakDamageNotification";

		public static string SmiteKillNotification = "SmitePassive.SmiteKillNotification";

		[SerializeField]
		private GameObject smiteFXPrefab;

		[SerializeField]
		private float range;

		[SerializeField]
		private int baseDamage;

		[SerializeField]
		private int damageBonusPerHP;

		[SerializeField]
		private SoundEffectSO soundFX;

		private ObjectPooler OP;

		private Ammo ammo;

		private void OnAmmoChanged(int ammoAmount)
		{
			if (ammoAmount == 0)
			{
				StartCoroutine(SmiteCR());
			}
		}

		private void Start()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(smiteFXPrefab.name, smiteFXPrefab, 50);
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			ammo = componentInParent.ammo;
			ammo.OnAmmoChanged.AddListener(OnAmmoChanged);
		}

		private void OnDestroy()
		{
			ammo.OnAmmoChanged.RemoveListener(OnAmmoChanged);
		}

		private IEnumerator SmiteCR()
		{
			yield return new WaitForSeconds(0.1f);
			Vector2 point = base.transform.position;
			Collider2D[] enemies = Physics2D.OverlapCircleAll(point, range, 1 << (int)TagLayerUtil.Enemy);
			soundFX?.Play();
			Collider2D[] array = enemies;
			foreach (Collider2D collider2D in array)
			{
				GameObject pooledObject = OP.GetPooledObject(smiteFXPrefab.name);
				pooledObject.transform.position = collider2D.transform.position + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0f);
				pooledObject.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(-45f, 45f));
				pooledObject.transform.position = collider2D.transform.position;
				pooledObject.SetActive(value: true);
			}
			yield return new WaitForSeconds(0.1f);
			array = enemies;
			for (int i = 0; i < array.Length; i++)
			{
				Health component = array[i].GetComponent<Health>();
				int num = baseDamage.NotifyModifiers(SmiteTweakDamageNotification, this);
				component.HPChange(-1 * num);
				this.PostNotification(SmiteKillNotification);
			}
		}
	}
}
