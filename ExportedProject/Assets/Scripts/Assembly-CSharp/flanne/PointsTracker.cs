using System;

namespace flanne
{
	public static class PointsTracker
	{
		public static EventHandler<int> PointsChangedEvent;

		private static int _pts;

		public static int pts
		{
			get
			{
				return _pts;
			}
			set
			{
				_pts = value;
				PointsChangedEvent?.Invoke(typeof(PointsTracker), _pts);
			}
		}
	}
}
