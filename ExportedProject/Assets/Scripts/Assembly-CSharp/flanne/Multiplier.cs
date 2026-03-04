using System;
using UnityEngine;

namespace flanne
{
	public class Multiplier
	{
		private float bonus;

		private float reduction = 1f;

		public float value => (1f + bonus) * reduction;

		public event EventHandler<float> ChangedEvent;

		public void Increase(float amount)
		{
			if (amount >= 0f)
			{
				bonus += amount;
				this.ChangedEvent?.Invoke(this, value);
			}
			else
			{
				Decrease(Mathf.Abs(amount));
			}
		}

		public void Decrease(float amount)
		{
			if (amount >= 0f)
			{
				reduction *= 1f - amount;
				this.ChangedEvent?.Invoke(this, value);
			}
			else
			{
				Decrease(Mathf.Abs(amount));
			}
		}

		public void ChangeBonus(float amount)
		{
			bonus += amount;
			this.ChangedEvent?.Invoke(this, value);
		}
	}
}
