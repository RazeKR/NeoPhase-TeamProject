using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace flanne.UI
{
	public class ChestUIController : MonoBehaviour
	{
		public EventHandler TakeClickEvent;

		public EventHandler LeaveClickEvent;

		[SerializeField]
		private ParticleSystem coinParticles;

		[SerializeField]
		private SpriteRenderer chestRenderer;

		[SerializeField]
		private Animator chestAnimator;

		[SerializeField]
		private PowerupWidget powerupWidget;

		[SerializeField]
		private Panel powerupIconPanel;

		[SerializeField]
		private Panel powerupDescriptionPanel;

		[SerializeField]
		private Panel takeButtonPanel;

		[SerializeField]
		private Button takeButton;

		[SerializeField]
		private Panel leaveButtonPanel;

		[SerializeField]
		private Button leaveButton;

		[SerializeField]
		private ScreenFlash screenFlash;

		[SerializeField]
		private SoundEffectSO chestLeadupSFX;

		[SerializeField]
		private SoundEffectSO chestOpenSFX;

		[SerializeField]
		private float chestOpenTiming;

		private void OnTakeClick()
		{
			TakeClickEvent?.Invoke(this, null);
		}

		private void OnLeaveClick()
		{
			LeaveClickEvent?.Invoke(this, null);
		}

		public void SetToPowerup(Powerup powerup)
		{
			powerupWidget.SetProperties(new PowerupProperties(powerup));
		}

		public void Show()
		{
			chestRenderer.enabled = true;
			chestAnimator.Play("A_ChestAnimation", -1, 0f);
			StartCoroutine(WaitToChestOpen());
			takeButton.onClick.AddListener(OnTakeClick);
			leaveButton.onClick.AddListener(OnLeaveClick);
		}

		public void Hide()
		{
			chestRenderer.enabled = false;
			powerupIconPanel.Hide();
			powerupDescriptionPanel.Hide();
			takeButtonPanel.Hide();
			leaveButtonPanel.Hide();
			takeButton.onClick.RemoveListener(OnTakeClick);
			leaveButton.onClick.RemoveListener(OnLeaveClick);
		}

		private IEnumerator WaitToChestOpen()
		{
			chestLeadupSFX?.Play();
			yield return new WaitForSecondsRealtime(chestOpenTiming);
			chestOpenSFX?.Play();
			coinParticles.Play();
			powerupIconPanel.Show();
			powerupIconPanel.transform.localPosition = Vector3.zero;
			LeanTween.moveLocalY(powerupIconPanel.gameObject, 50f, 0.5f).setIgnoreTimeScale(useUnScaledTime: true).setEase(LeanTweenType.easeOutBack);
			screenFlash.Flash(3);
			yield return new WaitForSecondsRealtime(0.1f);
			powerupDescriptionPanel.Show();
			yield return new WaitForSecondsRealtime(1f);
			takeButtonPanel.Show();
			leaveButtonPanel.Show();
		}
	}
}
