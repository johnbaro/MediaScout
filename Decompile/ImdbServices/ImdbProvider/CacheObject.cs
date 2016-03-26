using System;

namespace ImdbProvider
{
	[Serializable]
	public class CacheObject
	{
		public DateTime Created
		{
			get;
			set;
		}

		public object Object
		{
			get;
			set;
		}
	}
}
