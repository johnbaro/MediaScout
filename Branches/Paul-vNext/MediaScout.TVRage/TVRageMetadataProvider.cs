using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaScout.Interfaces;
using System.Xml;

namespace MediaScout.TVRage
{
    public class TVRageMetadataProvider : ITVMetadataProvider
    {
        private String urlSeriesID = @"http://services.tvrage.com/feeds/full_search.php?show=";
        private String urlMetadata = @"http://services.tvrage.com/feeds/full_show_info.php?sid=";

        private String CacheDirectory = "";

        public TVRageMetadataProvider()
        {
            CacheDirectory = Helpers.StorageHelper.GetCacheDirectory("TVRage");
        }

        public string Name
        {
            get { return "TVRage"; }
        }

        public Season GetSeason(string SeasonID = "", int SeasonNumber = -1, string SeriesName = "", string Language = "en")
        {
            throw new NotImplementedException();
        }

        public Episode GetEpisode(string EpisodeID = "", int EpisodeNumber = -1, int SeasonNumber = -1, string SeriesName = "", string Language = "en")
        {
            throw new NotImplementedException();
        }

        public IList<TVShow> Search(string SeriesName, string Language = "en")
        {
            List<TVShow> tvshows = new List<TVShow>();

            try
            {
                XmlDocument xdoc = new XmlDocument();
                XmlNode node;

                xdoc.Load(urlSeriesID + SeriesName);
                node = xdoc.DocumentElement;

                XmlNodeList xnl = node.SelectNodes("/Results/show");
                for (int i = 0; i < xnl.Count; i++)
                {
                    TVShow t = new TVShow();

                    if (xnl[i]["name"] != null)
                        t.SeriesName = xnl[i]["name"].InnerText;

                    if (xnl[i]["started"] != null)
                        t.Overview = xnl[i]["started"].InnerText;

                    if (xnl[i]["showid"] != null)
                        t.id = xnl[i]["showid"].InnerText;

                    tvshows.Add(t);
                }
            }
            catch
            {

            }

            return tvshows;
        }

        public TVShow GetTVShow(string TVShowID, string Language = "en")
        {
            throw new NotImplementedException();
        }
    }
}
