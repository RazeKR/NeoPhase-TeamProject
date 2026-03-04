using UnityEngine;

namespace flanne
{
	public class StatsHolder : MonoBehaviour
	{
		private StatMod[] _data = new StatMod[21];

		public StatMod this[StatType s] => _data[(int)s];

		private void Awake()
		{
			for (int i = 0; i < _data.Length; i++)
			{
				_data[i] = new StatMod();
			}
		}
	}
}
