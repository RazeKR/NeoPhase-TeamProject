using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class TriggerAnimationOnToggle : MonoBehaviour
	{
		[SerializeField]
		private Toggle toggle;

		[SerializeField]
		private Animator animator;

		[SerializeField]
		private string toggleOnTrigger;

		[SerializeField]
		private string toggleOffTrigger;

		private void Start()
		{
			toggle.onValueChanged.AddListener(delegate
			{
				ToggleValueChanged(toggle);
			});
		}

		private void ToggleValueChanged(Toggle change)
		{
			TriggerAnimation();
		}

		private void TriggerAnimation()
		{
			if (toggle.isOn)
			{
				animator.ResetTrigger(toggleOffTrigger);
				animator.SetTrigger(toggleOnTrigger);
			}
			else
			{
				animator.ResetTrigger(toggleOnTrigger);
				animator.SetTrigger(toggleOffTrigger);
			}
		}
	}
}
