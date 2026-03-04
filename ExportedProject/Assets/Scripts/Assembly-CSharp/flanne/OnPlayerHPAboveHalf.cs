using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace flanne
{
	public class OnPlayerHPAboveHalf : MonoBehaviour
	{
		[SerializeField]
		private bool orEqualTo;

		public UnityEvent onHPBelowHalf;

		private PlayerHealth health;

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			health = componentInParent.playerHealth;
			health.onHealthChangedTo.AddListener(OnHPChanged);
			StartCoroutine(WaitToCheckHPCR());
		}

		private void OnDestroy()
		{
			health.onHealthChangedTo.RemoveListener(OnHPChanged);
		}

		private void OnHPChanged(int newHP)
		{
			CheckHP(newHP);
		}

		private void CheckHP(int hp)
		{
			if (orEqualTo)
			{
				if ((float)hp / (float)health.maxHP >= 0.5f)
				{
					onHPBelowHalf?.Invoke();
				}
			}
			else if ((float)hp / (float)health.maxHP > 0.5f)
			{
				onHPBelowHalf?.Invoke();
			}
		}

		private IEnumerator WaitToCheckHPCR()
		{
			yield return null;
			CheckHP(health.hp);
		}
	}
}
