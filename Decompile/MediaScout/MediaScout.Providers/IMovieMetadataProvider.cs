using System;

namespace MediaScout.Providers
{
	public interface IMovieMetadataProvider : IMetadataProvider
	{
		MovieXML[] Search(string Name, string Year);

		MovieXML Get(string ID);
	}
}
