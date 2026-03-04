using System;
using System.Collections.Generic;
using UnityEngine;

namespace flanne.UI
{
	public class Menu : MonoBehaviour
	{
		[SerializeField]
		protected List<MenuEntry> entries;

		public event EventHandler<InfoEventArgs<int>> ClickEvent;

		public event EventHandler<InfoEventArgs<int>> SelectEvent;

		public virtual void OnEntryClicked(int index)
		{
			if (this.ClickEvent != null)
			{
				this.ClickEvent(this, new InfoEventArgs<int>(index));
			}
		}

		public virtual void OnEntrySelected(int index)
		{
			if (this.SelectEvent != null)
			{
				this.SelectEvent(this, new InfoEventArgs<int>(index));
			}
		}

		private void Start()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				int closureIndex = i;
				entries[closureIndex].onClick.AddListener(delegate
				{
					OnEntryClicked(closureIndex);
				});
				entries[closureIndex].onSelect.AddListener(delegate
				{
					OnEntrySelected(closureIndex);
				});
			}
		}

		private void OnDestroy()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				int closureIndex = i;
				entries[closureIndex].onClick.RemoveListener(delegate
				{
					OnEntryClicked(closureIndex);
				});
				entries[closureIndex].onSelect.RemoveListener(delegate
				{
					OnEntrySelected(closureIndex);
				});
			}
		}

		public void SetProperties<T>(int index, T properties) where T : IUIProperties
		{
			if (index >= 0 && index < entries.Count)
			{
				Widget<T> component = entries[index].GetComponent<Widget<T>>();
				if (component != null)
				{
					component.SetProperties(properties);
				}
			}
			else
			{
				Debug.LogError("Cannot set property of entry " + index + ". Index out of bounds.");
			}
		}

		public void Select(int index)
		{
			if (index >= 0 && index < entries.Count)
			{
				entries[index].Select();
			}
			else
			{
				Debug.LogError("Cannot select menu entry " + index + ". Index out of bounds.");
			}
		}

		public int SelectFirstAvailable()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].interactable)
				{
					Select(i);
					return i;
				}
			}
			return -1;
		}

		public virtual void Lock(int index)
		{
			entries[index].interactable = false;
		}

		public void UnLock(int index)
		{
			entries[index].interactable = true;
		}

		public void SetEntryActive(int index, bool active)
		{
			entries[index].gameObject.SetActive(active);
		}

		public void UnlockAll()
		{
			foreach (MenuEntry entry in entries)
			{
				entry.interactable = true;
			}
		}

		public Vector2 GetEntryPosition(int index)
		{
			return entries[index].transform.position;
		}

		public MenuEntry GetEntry(int index)
		{
			return entries[index];
		}
	}
}
