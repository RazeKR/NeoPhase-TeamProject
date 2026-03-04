using System.Collections.Generic;
using UnityEngine;
using flanne.RuneSystem;

namespace flanne.UI
{
	public class RuneTreeUI : MonoBehaviour
	{
		[SerializeField]
		private RuneRowUI[] rows;

		private void OnLevelChange()
		{
			Refresh();
		}

		private void Start()
		{
			RuneRowUI[] array = rows;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].onLevelChanged.AddListener(OnLevelChange);
			}
		}

		private void OnDestroy()
		{
			RuneRowUI[] array = rows;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].onLevelChanged.AddListener(OnLevelChange);
			}
		}

		public int[] GetSelections()
		{
			int[] array = new int[rows.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = rows[i].toggleIndex;
			}
			return array;
		}

		public void SetSelections(int[] selections)
		{
			if (selections != null)
			{
				for (int i = 0; i < selections.Length; i++)
				{
					rows[i].toggleIndex = selections[i];
				}
				Refresh();
			}
		}

		public List<RuneData> GetActiveRunes()
		{
			List<RuneData> list = new List<RuneData>();
			for (int i = 0; i < rows.Length; i++)
			{
				RuneData toggledRune = rows[i].toggledRune;
				if (toggledRune != null)
				{
					list.Add(toggledRune);
				}
			}
			return list;
		}

		public void Refresh()
		{
			int num = 0;
			RuneRowUI[] array = rows;
			foreach (RuneRowUI runeRowUI in array)
			{
				num += runeRowUI.totalLevels;
			}
			array = rows;
			foreach (RuneRowUI runeRowUI2 in array)
			{
				if (runeRowUI2.levelRequirement <= num)
				{
					runeRowUI2.UnLock();
				}
				else
				{
					runeRowUI2.Lock();
				}
			}
		}
	}
}
