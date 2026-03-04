using System;

namespace flanne
{
	[Serializable]
	public struct LocalizedString
	{
		public string key;

		public LocalizedString(string key)
		{
			this.key = key;
		}

		public static implicit operator LocalizedString(string key)
		{
			return new LocalizedString(key);
		}
	}
}
