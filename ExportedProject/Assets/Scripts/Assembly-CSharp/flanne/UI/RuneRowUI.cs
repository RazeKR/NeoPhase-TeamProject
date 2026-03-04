using System;
using UnityEngine;
using UnityEngine.Events;
using flanne.RuneSystem;

namespace flanne.UI
{
	public class RuneRowUI : MonoBehaviour
	{
		[NonSerialized]
		public UnityEvent onLevelChanged;

		public int levelRequirement;

		[SerializeField]
		private RuneUnlocker[] runes;

		public int toggleIndex
		{
			get
			{
				for (int i = 0; i < runes.Length; i++)
				{
					if (runes[i].toggleOn)
					{
						return i;
					}
				}
				return -1;
			}
			set
			{
				if (value == -1)
				{
					RuneUnlocker[] array = runes;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].toggleOn = false;
					}
				}
				else
				{
					runes[value].toggleOn = true;
				}
			}
		}

		public RuneData toggledRune
		{
			get
			{
				int num = toggleIndex;
				if (num != -1)
				{
					return runes[num].rune.data;
				}
				return null;
			}
		}

		public int totalLevels
		{
			get
			{
				int num = 0;
				RuneUnlocker[] array = runes;
				foreach (RuneUnlocker runeUnlocker in array)
				{
					num += runeUnlocker.level;
				}
				return num;
			}
		}

		private void OnLevelChange()
		{
			onLevelChanged?.Invoke();
		}

		private void Awake()
		{
			onLevelChanged = new UnityEvent();
		}

		private void Start()
		{
			RuneUnlocker[] array = runes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].onLevel.AddListener(OnLevelChange);
			}
		}

		private void OnDestroy()
		{
			RuneUnlocker[] array = runes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].onLevel.RemoveListener(OnLevelChange);
			}
		}

		public void Lock()
		{
			RuneUnlocker[] array = runes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].locked = true;
			}
		}

		public void UnLock()
		{
			RuneUnlocker[] array = runes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].locked = false;
			}
		}
	}
}
