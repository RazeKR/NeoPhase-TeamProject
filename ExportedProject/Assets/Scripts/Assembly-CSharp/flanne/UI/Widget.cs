using UnityEngine;

namespace flanne.UI
{
	public abstract class Widget<T> : MonoBehaviour where T : IUIProperties
	{
		private Panel panel;

		private void Start()
		{
			panel = GetComponent<Panel>();
		}

		public virtual void Show()
		{
			if (panel != null)
			{
				panel.Show();
			}
		}

		public virtual void Hide()
		{
			if (panel != null)
			{
				panel.Hide();
			}
		}

		public abstract void SetProperties(T properties);
	}
}
