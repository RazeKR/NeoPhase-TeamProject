using UnityEngine;

namespace flanne
{
	public class SpawnObjectTowardsCursorOnReload : MonoBehaviour
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private float distanceFromPlayerToSpawn;

		[SerializeField]
		private SoundEffectSO soundFX;

		private ShootingCursor SC;

		private ObjectPooler OP;

		private PlayerController player;

		private Ammo ammo;

		private void OnReload()
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 vector2 = player.transform.position;
			Vector2 vector3 = (vector - vector2).normalized * distanceFromPlayerToSpawn;
			Vector2 vector4 = vector2 + vector3;
			GameObject pooledObject = OP.GetPooledObject(prefab.name);
			pooledObject.transform.position = vector4;
			pooledObject.SetActive(value: true);
			soundFX?.Play();
		}

		private void Start()
		{
			SC = ShootingCursor.Instance;
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(prefab.name, prefab, 100);
			player = GetComponentInParent<PlayerController>();
			ammo = player.ammo;
			ammo.OnReload.AddListener(OnReload);
		}

		private void OnDestroy()
		{
			ammo.OnReload.RemoveListener(OnReload);
		}
	}
}
