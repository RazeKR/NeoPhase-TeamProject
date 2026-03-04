using System;

namespace flanne
{
	public class BoolToggle
	{
		private int _flip;

		public bool value
		{
			get
			{
				if (_flip > 0)
				{
					return !defaultValue;
				}
				return defaultValue;
			}
		}

		public bool defaultValue { get; private set; }

		public int flips => _flip;

		public event EventHandler<bool> ToggleEvent;

		public BoolToggle(bool b)
		{
			defaultValue = b;
		}

		public void Flip()
		{
			_flip++;
			if (this.ToggleEvent != null)
			{
				this.ToggleEvent(this, value);
			}
		}

		public void UnFlip()
		{
			_flip--;
			if (this.ToggleEvent != null)
			{
				this.ToggleEvent(this, value);
			}
		}

		public int GetFlipAmount()
		{
			return _flip;
		}
	}
}
