using System;
using System.Collections.Generic;
using MediaScout.Providers;
using System.Xml;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace MediaScout.Providers
{
    public class TVRageProvider : ITVMetadataProvider
    {
        public event MediaScoutMessage.Message Message;

        private String urlSeriesID = @"http://services.tvrage.com/feeds/full_search.php?show=";
        private String urlMetadata = @"http://services.tvrage.com/feeds/full_show_info.php?sid=";
        private String defaultLanguage = "en";

        public string name { get { return "TVRage"; } }
        public string version { get { return "0.1"; } }
        public string url { get { return "http://www.TVRage.com"; } }

        public String defaultCacheDir = System.Environment.CurrentDirectory + @"\Cache\TVCache\";
        public DateTime dtDefaultCache = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));

        public TVShow[] Search(String SeriesName)
        {
            return Search(SeriesName, defaultLanguage);
        }

        public TVShow[] Search(String SeriesName, String Language)
        {
            if (Message != null)
                Message("Querying TVRage ID for " + SeriesName, MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);

            XmlDocument xdoc = new XmlDocument();
            XmlNode node;
            List<TVShow> tvshows = new List<TVShow>();

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
            return tvshows.ToArray();
        }

        public TVShow GetTVShow(String TVShowID)
        {
            return GetTVShow(TVShowID, defaultLanguage, defaultCacheDir, dtDefaultCache);
        }
        public TVShow GetTVShow(String TVShowID, String language)
        {
            return GetTVShow(TVShowID, language, defaultCacheDir, dtDefaultCache);
        }

        public TVShow GetTVShow(String TVShowID, String Language, String CacheDirectory, DateTime dtCacheTime)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlNode node;
            XmlNodeList nodeList;
            TVShow s;

            if (CacheDirectory == null)
                CacheDirectory = defaultCacheDir;

            if (dtCacheTime == null)
                dtCacheTime = dtDefaultCache;

            if (Language == null)
                Language = defaultLanguage;

            try
            {
                Debug.WriteLine("Fetching " + urlMetadata + TVShowID);
                xdoc.Load(urlMetadata + TVShowID);

                node = xdoc.DocumentElement;
                
                //Create Series/Fetch Series Metadata
                nodeList = node.SelectNodes("/Show");
                String SeriesName = nodeList[0].SelectSingleNode("name").InnerText;
                Debug.WriteLine("Series name: " + SeriesName);
                String SeriesPosterUrl = nodeList[0].SelectSingleNode("image").InnerText;
                Debug.WriteLine("Poster: " + SeriesPosterUrl);

                s = new TVShow(TVShowID, SeriesName, SeriesPosterUrl, null);
                s.Network = nodeList[0].SelectSingleNode("network").InnerText;
                s.Runtime = nodeList[0].SelectSingleNode("runtime").InnerText;
                s.FirstAired = nodeList[0].SelectSingleNode("started").InnerText;

                //Deal with the XML for specific episodes
                nodeList = node.SelectNodes("/Show/Episodelist/Season");
                foreach (XmlNode xmlSeason in nodeList)
                {
                    Debug.WriteLine(xmlSeason.Name + " " + xmlSeason.Attributes["no"].Value);
                    Int32 SeasonNumber = Int32.Parse(xmlSeason.Attributes["no"].Value);  // <Show><Episodelist><Season no="1">

                    foreach (XmlNode xmlEpisode in xmlSeason)
                    {
                        //Extract metadata for episode/seasons
                        String EpisodeXML = "<Item>" + xmlEpisode.InnerXml + "</Item>";

                        Int32 EpisodeNumber = Int32.Parse(xmlEpisode.SelectSingleNode("seasonnum").InnerText);
                        String EpisodeName = xmlEpisode.SelectSingleNode("title").InnerText;
                        String AirDate = xmlEpisode.SelectSingleNode("airdate").InnerText;
                        String ProdNum = xmlEpisode.SelectSingleNode("prodnum").InnerText;
                        String EpisodeID = xmlEpisode.SelectSingleNode("link").InnerText;
                        EpisodeID = EpisodeID.Substring(EpisodeID.LastIndexOf("/")).Trim('/');
                        Debug.WriteLine(xmlEpisode.Name + " " + EpisodeID + " " + " " + SeasonNumber + " " + EpisodeNumber + " " + AirDate + " " + EpisodeName);

                        if (!s.Seasons.ContainsKey(SeasonNumber))
                        {
                            s.Seasons.Add(SeasonNumber, new Season(SeasonNumber, TVShowID));
                        }

                        if (!s.Seasons[SeasonNumber].Episodes.ContainsKey(EpisodeNumber))
                        {
                            Episode ep = new Episode(EpisodeNumber, EpisodeName, null);
                            ep.FirstAired = AirDate;
                            ep.ProductionCode = ProdNum;
                            ep.EpisodeID = EpisodeID;
                            ep.SeasonNumber = SeasonNumber.ToString();
                            s.Seasons[SeasonNumber].Episodes.Add(EpisodeNumber, ep);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                s = null;
                throw new Exception(ex.Message + ex.StackTrace);
            }

            return s;

        }

        #region ITVMetadataProvider Members


        public Season GetSeason(string SeasonID)
        {
            throw new NotImplementedException();
        }

        public Episode GetEpisode(string EpisodeID)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
