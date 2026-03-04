using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace flanne.UI
{
	public class RuneUnlocker : TieredUnlockable, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public RuneIcon rune;

		[SerializeField]
		private InputActionAsset inputs;

		[SerializeField]
		private Toggle toggle;

		[SerializeField]
		private Button button;

		[SerializeField]
		private Slider levelupSlider;

		[SerializeField]
		private ParticleSystem levelupParticles;

		[SerializeField]
		private int maxLevel = 5;

		[SerializeField]
		private float holdToUnlockTime;

		public UnityEvent onLevel;

		private InputAction _submitAction;

		private InputAction _clickAction;

		private IEnumerator _pressAndHoldCoroutine;

		private bool _isSelected;

		private bool _isPointerOn;

		public override int level
		{
			get
			{
				return rune.data.level;
			}
			set
			{
				rune.data.level = value;
				CheckRuneLevelReq();
				rune.Refresh();
				onLevel?.Invoke();
			}
		}

		public bool toggleOn
		{
			get
			{
				return toggle.isOn;
			}
			set
			{
				toggle.isOn = value;
			}
		}

		public bool locked
		{
			get
			{
				return !button.interactable;
			}
			set
			{
				button.interactable = !value;
			}
		}

		public int costPerLevel => rune.data.costPerLevel;

		public void OnPointerEnter(PointerEventData eventData)
		{
			_isPointerOn = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_isPointerOn = false;
		}

		public void OnSelect(BaseEventData eventData)
		{
			_isSelected = true;
		}

		public void OnDeselect(BaseEventData data)
		{
			_isSelected = false;
			ReleasePress();
		}

		public void OnSubmitChanged(InputAction.CallbackContext context)
		{
			if (context.ReadValue<float>() == 0f)
			{
				ReleasePress();
			}
			else
			{
				StartPress();
			}
		}

		public void OnClickChanged(InputAction.CallbackContext context)
		{
			if (context.ReadValue<float>() == 0f)
			{
				ReleasePress();
			}
			else if (_isPointerOn)
			{
				StartPress();
			}
		}

		private void Start()
		{
			CheckRuneLevelReq();
			_submitAction = inputs.FindActionMap("UI").FindAction("Submit");
			_clickAction = inputs.FindActionMap("UI").FindAction("Click");
			_submitAction.performed += OnSubmitChanged;
			_clickAction.performed += OnClickChanged;
		}

		private void OnDestroy()
		{
			_submitAction.performed -= OnSubmitChanged;
			_clickAction.performed -= OnClickChanged;
		}

		private void CheckRuneLevelReq()
		{
			if (rune.data.level == 0)
			{
				toggle.interactable = false;
			}
			else
			{
				toggle.interactable = true;
			}
		}

		private void LevelUp()
		{
			level++;
			rune.Refresh();
			toggle.isOn = true;
			levelupSlider.value = 0f;
			levelupParticles.Play();
			PointsTracker.pts -= costPerLevel;
			onLevel?.Invoke();
		}

		private void StartPress()
		{
			ReleasePress();
			if (!locked)
			{
				if (_isSelected && toggle.interactable)
				{
					toggle.isOn = !toggle.isOn;
				}
				if (_isSelected && level < maxLevel && costPerLevel < PointsTracker.pts)
				{
					_pressAndHoldCoroutine = PressAndHoldCR();
					StartCoroutine(_pressAndHoldCoroutine);
				}
			}
		}

		private void ReleasePress()
		{
			if (_pressAndHoldCoroutine != null)
			{
				StopCoroutine(_pressAndHoldCoroutine);
				_pressAndHoldCoroutine = null;
			}
			levelupSlider.value = 0f;
		}

		private IEnumerator PressAndHoldCR()
		{
			float timer = 0f;
			while (timer < holdToUnlockTime)
			{
				yield return null;
				timer += Time.deltaTime;
				levelupSlider.value = timer / holdToUnlockTime;
			}
			LevelUp();
			_pressAndHoldCoroutine = null;
		}
	}
}
