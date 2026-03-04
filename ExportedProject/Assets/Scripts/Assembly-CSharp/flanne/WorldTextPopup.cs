using TMPro;
using UnityEngine;

namespace flanne
{
	public class WorldTextPopup : MonoBehaviour
	{
		[SerializeField]
		private TextMeshPro tmp;

		[SerializeField]
		private float lifetime;

		[SerializeField]
		private bool destroyOnStop;

		private float timer;

		private Vector3 newPos;

		private void OnEnable()
		{
			timer = 0f;
			base.transform.position = new Vector3(base.transform.position.x + Random.Range(-0.5f, 0.5f), base.transform.position.y + Random.Range(-0.5f, 0.5f));
			newPos = new Vector3(base.transform.position.x + Random.Range(-0.5f, 0.5f), base.transform.position.y + 0.5f, 0f);
			base.transform.localScale = Vector3.one;
		}

		private void Update()
		{
			if (timer < lifetime)
			{
				timer += Time.deltaTime;
				base.transform.position = Vector3.MoveTowards(base.transform.position, newPos, (1f - timer / lifetime) / 2f * Time.deltaTime);
				if (timer < lifetime * 0.3f)
				{
					base.transform.localScale = Vector3.one + Vector3.one * (timer / 0.3f);
				}
			}
			else if (destroyOnStop)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
