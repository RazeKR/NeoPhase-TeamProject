using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace flanne
{
	public class Unlockable : MonoBehaviour
	{
		[SerializeField]
		private int unlockCost;

		[SerializeField]
		private Selectable targetSelectable;

		[SerializeField]
		private Image targetSprite;

		[SerializeField]
		private Image lockSprite;

		[SerializeField]
		private Material lockMaterial;

		[SerializeField]
		private bool unbuyable;

		[SerializeField]
		private Button unlockButton;

		[SerializeField]
		private TMP_Text unlockCostTMP;

		[SerializeField]
		private SoundEffectSO unlockSFX;

		private Material _originalMaterial;

		private bool _isLocked;

		public bool IsLocked => _isLocked;

		private void Awake()
		{
			_originalMaterial = targetSprite.material;
			if (!unbuyable)
			{
				unlockCostTMP.text = ("Unlock<br>" + unlockCost) ?? "";
			}
		}

		public void Lock()
		{
			targetSelectable.interactable = false;
			targetSprite.material = lockMaterial;
			lockSprite.enabled = true;
			_isLocked = true;
		}

		public void UnlockWithPoints()
		{
			if (PointsTracker.pts >= unlockCost)
			{
				unlockSFX?.Play();
				PointsTracker.pts -= unlockCost;
				targetSelectable.Select();
				Unlock();
			}
		}

		public void Unlock()
		{
			targetSelectable.interactable = true;
			targetSprite.material = _originalMaterial;
			lockSprite.enabled = false;
			_isLocked = false;
			if ((bool)unlockButton)
			{
				unlockButton.gameObject.SetActive(value: false);
			}
		}
	}
}
