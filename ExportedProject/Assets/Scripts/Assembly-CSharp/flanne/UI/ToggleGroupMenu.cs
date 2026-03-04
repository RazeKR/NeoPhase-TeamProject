using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace flanne.UI
{
	public class ToggleGroupMenu<T> : MonoBehaviour where T : ScriptableObject
	{
		[SerializeField]
		private Toggle[] toggles;

		[SerializeField]
		private ToggleGroup toggleGroup;

		[SerializeField]
		private GameObject descriptionObj;

		[SerializeField]
		private SoundEffectSO hoverSFX;

		[SerializeField]
		private SoundEffectSO clickSFX;

		[SerializeField]
		private bool saveLastSelectionToPlayerPrefs;

		[SerializeField]
		private string playerPrefsKey;

		[SerializeField]
		private PlayerInput playerInput;

		private const string gamepadScheme = "Gamepad";

		private const string mouseScheme = "Keyboard&Mouse";

		private DataUIBinding<T> description;

		private List<DataUIBinding<T>> entries;

		private List<PointerDetector> pointerDetectors;

		public T toggledData
		{
			get
			{
				Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
				if ((object)toggle == null)
				{
					return null;
				}
				return toggle.GetComponent<DataUIBinding<T>>().data;
			}
		}

		public int currIndex { get; private set; }

		public event EventHandler<T> ConfirmEvent;

		private void OnPointerEnter(int index)
		{
			if (description != null)
			{
				description.data = entries[index].data;
			}
			hoverSFX?.Play();
		}

		private void OnPointerExit()
		{
			if (description != null)
			{
				description.data = toggledData;
			}
		}

		private void OnToggleChanged(int index)
		{
			if (saveLastSelectionToPlayerPrefs)
			{
				PlayerPrefs.SetInt(playerPrefsKey, index);
			}
			if (description != null)
			{
				description.data = toggledData;
			}
			if (playerInput != null && playerInput.currentControlScheme == "Gamepad" && toggles[index].isOn && currIndex == index)
			{
				OnConfirmClicked();
			}
			if (currIndex != index)
			{
				currIndex = index;
				clickSFX?.Play();
			}
		}

		public void OnConfirmClicked()
		{
			this.ConfirmEvent?.Invoke(this, toggledData);
		}

		private void Start()
		{
			if (descriptionObj != null)
			{
				description = descriptionObj.GetComponent<DataUIBinding<T>>();
			}
			entries = new List<DataUIBinding<T>>();
			pointerDetectors = new List<PointerDetector>();
			for (int i = 0; i < toggles.Length; i++)
			{
				int index = i;
				PointerDetector component = toggles[i].GetComponent<PointerDetector>();
				component.onEnter.AddListener(delegate
				{
					OnPointerEnter(index);
				});
				component.onExit.AddListener(OnPointerExit);
				component.onSelect.AddListener(delegate
				{
					OnPointerEnter(index);
				});
				component.onDeselect.AddListener(OnPointerExit);
				pointerDetectors.Add(component);
				DataUIBinding<T> component2 = toggles[i].GetComponent<DataUIBinding<T>>();
				entries.Add(component2);
			}
			int value;
			if (saveLastSelectionToPlayerPrefs)
			{
				value = PlayerPrefs.GetInt(playerPrefsKey, 0);
				value = Mathf.Clamp(value, 0, entries.Count - 1);
			}
			else
			{
				value = 0;
			}
			if (description != null)
			{
				description.data = entries[value].data;
			}
			toggles[value].isOn = true;
			currIndex = value;
			for (int num = 0; num < toggles.Length; num++)
			{
				int index2 = num;
				toggles[num].onValueChanged.AddListener(delegate
				{
					OnToggleChanged(index2);
				});
			}
		}

		public void SetData(int index, T data)
		{
			entries[index].data = data;
			if (description != null)
			{
				description.data = toggledData;
			}
		}

		public void SetActive(int index, bool isActive)
		{
			entries[index].gameObject.SetActive(isActive);
		}

		public void RefreshDescription()
		{
			if (description != null)
			{
				description.data = toggledData;
			}
		}

		public void RefreshToggleData()
		{
			foreach (DataUIBinding<T> entry in entries)
			{
				entry.Refresh();
			}
		}
	}
}
