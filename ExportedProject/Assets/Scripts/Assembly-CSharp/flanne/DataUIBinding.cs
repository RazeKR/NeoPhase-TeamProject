using UnityEngine;

namespace flanne
{
	public abstract class DataUIBinding<T> : MonoBehaviour where T : ScriptableObject
	{
		[SerializeField]
		private T _data;

		public T data
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
				Refresh();
			}
		}

		private void Start()
		{
			Refresh();
		}

		public abstract void Refresh();
	}
}
