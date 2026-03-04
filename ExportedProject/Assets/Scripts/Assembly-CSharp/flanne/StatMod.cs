using System;

namespace flanne
{
	public class StatMod
	{
		private int _flatBonus;

		private float _multiplierBonus;

		private float _multiplierReduction = 1f;

		public int FlatBonus => _flatBonus;

		public event EventHandler ChangedEvent;

		public float Modify(float baseValue)
		{
			return (baseValue + (float)_flatBonus) * (1f + _multiplierBonus) * _multiplierReduction;
		}

		public float ModifyInverse(float baseValue)
		{
			return (baseValue + (float)_flatBonus) / ((1f + _multiplierBonus) * _multiplierReduction);
		}

		public void AddFlatBonus(int value)
		{
			_flatBonus += value;
			this.ChangedEvent?.Invoke(this, null);
		}

		public void AddMultiplierBonus(float value)
		{
			_multiplierBonus += value;
			this.ChangedEvent?.Invoke(this, null);
		}

		public void AddMultiplierReduction(float value)
		{
			_multiplierReduction *= value;
			this.ChangedEvent?.Invoke(this, null);
		}
	}
}
