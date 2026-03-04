namespace flanne
{
	public class NonStackingBuff : BuffPlayerStats
	{
		private bool _isActive;

		public void ApplyNonStackBuff()
		{
			if (!_isActive)
			{
				ApplyBuff();
				_isActive = true;
			}
		}

		public void RemoveNonStackBuff()
		{
			if (_isActive)
			{
				RemoveBuff();
				_isActive = false;
			}
		}
	}
}
