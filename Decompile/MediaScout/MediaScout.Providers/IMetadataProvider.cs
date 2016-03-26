using System;

namespace MediaScout.Providers
{
	public interface IMetadataProvider
	{
		string name
		{
			get;
		}

		string version
		{
			get;
		}

		string url
		{
			get;
		}
	}
}
