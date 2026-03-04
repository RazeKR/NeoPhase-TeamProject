using System.Collections;
using UnityEngine;

namespace flanne.Pickups
{
	public class Pickup : MonoBehaviour
	{
		[SerializeField]
		private SoundEffectSO soundFX;

		private IEnumerator pickUpCoroutine;

		protected virtual void UsePickup(GameObject pickupper)
		{
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if ((other.tag == "Pickupper" || other.tag == "MapBounds") && pickUpCoroutine == null)
			{
				pickUpCoroutine = PickupCR(PlayerController.Instance.gameObject);
				StartCoroutine(pickUpCoroutine);
			}
		}

		private IEnumerator PickupCR(GameObject pickupper)
		{
			base.transform.SetParent(pickupper.transform);
			int tweenID = LeanTween.moveLocal(base.gameObject, Vector3.zero, 0.3f).setEase(LeanTweenType.easeInBack).id;
			while (LeanTween.isTweening(tweenID))
			{
				yield return null;
			}
			UsePickup(pickupper);
			if (soundFX != null)
			{
				soundFX.Play();
			}
			pickUpCoroutine = null;
			base.transform.SetParent(ObjectPooler.SharedInstance.transform);
			base.transform.localPosition = Vector3.zero;
			base.gameObject.SetActive(value: false);
		}
	}
}
