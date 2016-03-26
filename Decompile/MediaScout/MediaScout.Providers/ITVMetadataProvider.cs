using System;

namespace MediaScout.Providers
{
	internal interface ITVMetadataProvider : IMetadataProvider
	{
		TVShowXML[] Search(string Name, string Year);

		TVShowXML GetTVShow(string ID);

		EpisodeXML GetEpisode(string TVShowID, string SeasonID, string EpisodeID);
	}
}
