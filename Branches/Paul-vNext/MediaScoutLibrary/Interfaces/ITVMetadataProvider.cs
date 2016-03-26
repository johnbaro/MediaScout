using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MediaScout.Interfaces
{
    [InheritedExport()]
    public interface ITVMetadataProvider : IMediaMetadataProvider
    {
        IList<TVShow> Search(String SeriesName, String Language = "en");
        TVShow GetTVShow(String TVShowID, String Language = "en");

        Season GetSeason(String SeasonID = "", Int32 SeasonNumber = -1, String SeriesName = "", String Language="en");
        Episode GetEpisode(String EpisodeID = "", Int32 EpisodeNumber = -1, Int32 SeasonNumber = -1, String SeriesName = "", String Language = "en");
    }
}
