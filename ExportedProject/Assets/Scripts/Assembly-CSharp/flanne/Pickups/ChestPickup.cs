using System.Collections;
using UnityEngine;

namespace flanne.Pickups
{
	public class ChestPickup : MonoBehaviour
	{
		public static string ChestPickupEvent = "ChestPickup.ChestPickupEvent";

		[SerializeField]
		private int amountOfXP;

		[SerializeField]
		private string xpOPTag;

		[SerializeField]
		private SpriteRenderer spriteRenderer;

		[SerializeField]
		private Sprite chestOpen;

		[SerializeField]
		private Sprite chestClosed;

		[SerializeField]
		private SoundEffectSO xpSpawnSFX;

		private IEnumerator _xpFountainCoroutine;

		private ObjectPooler OP;

		private void Start()
		{
			_xpFountainCoroutine = null;
			OP = ObjectPooler.SharedInstance;
		}

		private void OnEnable()
		{
			spriteRenderer.sprite = chestClosed;
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if ((other.tag == "Player" || other.tag == "MapBounds") && _xpFountainCoroutine == null)
			{
				this.PostNotification(ChestPickupEvent, null);
				_xpFountainCoroutine = XPFountainCR();
				StartCoroutine(_xpFountainCoroutine);
			}
		}

		private IEnumerator XPFountainCR()
		{
			yield return new WaitForSeconds(0.1f);
			spriteRenderer.sprite = chestOpen;
			for (int i = 0; i < amountOfXP; i++)
			{
				GameObject pooledObject = OP.GetPooledObject(xpOPTag);
				pooledObject.transform.position = base.transform.position;
				pooledObject.SetActive(value: true);
				Vector3 to = new Vector3(pooledObject.transform.position.x + Random.Range(-1f, 1f), pooledObject.transform.position.y + Random.Range(-1f, 1f), 0f);
				LeanTween.move(pooledObject, to, 0.5f);
				xpSpawnSFX?.Play();
				yield return new WaitForSeconds(0.1f);
			}
			_xpFountainCoroutine = null;
			base.gameObject.SetActive(value: false);
		}
	}
}
