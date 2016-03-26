using System;
using System.Collections.Generic;
using System.Text;

namespace MediaScout.Providers
{
    interface ITVMetadataProvider : IMetadataProvider
    {
        event MediaScoutMessage.Message Message;

        TVShow[] Search(String SeriesName);
        TVShow GetTVShow(String TVShowID);
        Season GetSeason(String SeasonID);
        Episode GetEpisode(String EpisodeID);

    }
}
