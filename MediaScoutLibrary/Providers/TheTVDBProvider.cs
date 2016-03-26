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
    public class TheTVDBProvider : ITVMetadataProvider
    {
        public event MediaScoutMessage.Message Message;

        private const String APIKey = "4AD667B666AA62FA";
        private String urlSeriesID = @"http://www.thetvdb.com/api/GetSeries.php?seriesname=";
        private String urlMetadata = @"http://www.thetvdb.com/api/" + APIKey + "/series/";
        private String urlPoster = @"http://thetvdb.com/banners/";
        private String defaultLanguage = "en";

        public string name { get { return "TheTVDB"; } }
        public string version { get { return "0.1"; } }
        public string url { get { return "http://www.thetvdb.com"; } }

        public String defaultCacheDir =  System.Environment.CurrentDirectory  + @"\Cache\TVCache\";
        public DateTime dtDefaultCache = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));

        public TVShow[] Search(String SeriesName)
        {

            return Search(SeriesName, defaultLanguage);
        }

        public TVShow[] Search(String SeriesName, String Language)
        {
            if (Message != null)
                Message("Querying TV ID for " + SeriesName, MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);

            XmlDocument xdoc = new XmlDocument();
            XmlNode node;
            List<TVShow> tvshows = new List<TVShow>();


            xdoc.Load(urlSeriesID + SeriesName + "&language=" + Language);
            node = xdoc.DocumentElement;

            XmlNodeList xnl = node.SelectNodes("/Data/Series");
            for (int i = 0; i < xnl.Count; i++)
            {
                TVShow t = new TVShow();
                if (xnl[i]["seriesid"] != null)
                    t.SeriesID = xnl[i]["seriesid"].InnerText;

                if (xnl[i]["SeriesName"] != null)
                    t.SeriesName = xnl[i]["SeriesName"].InnerText;

                if (xnl[i]["Overview"] != null)
                    t.Overview = xnl[i]["Overview"].InnerText;

                if (xnl[i]["id"] != null)
                    t.id = xnl[i]["id"].InnerText;

                if (xnl[i]["banner"] != null)
                    t.SeriesBannerUrl = urlPoster + xnl[i]["banner"].InnerText;

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

            try
            {
                if (File.Exists(CacheDirectory + "\\" + TVShowID + ".xml") && (DateTime.Compare(File.GetLastWriteTime(CacheDirectory + "\\" + TVShowID + ".xml"), dtCacheTime) > 0))
                {
                    if (Message != null)
                        Message("Loading from cache", MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);

                    xdoc.Load(CacheDirectory + "\\" + TVShowID + ".xml");
                }
                else
                {
                    xdoc.Load(urlMetadata + TVShowID + "/all/" + Language + ".xml");
                }

                node = xdoc.DocumentElement;
                
                if (Message != null)
                    Message("Metadata retrieved.  Processing...", MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);

                //Create Series/Fetch Series Metadata
                nodeList = node.SelectNodes("/Data/Series");
                //String tvcomID = nodeList[0].SelectSingleNode("SeriesID").InnerText;
                String SeriesName = nodeList[0].SelectSingleNode("SeriesName").InnerText;
                String SeriesPosterUrl = nodeList[0].SelectSingleNode("poster").InnerText;
                String SeriesBannerUrl = nodeList[0].SelectSingleNode("banner").InnerText; 
                String SeriesMetadata = xdoc.ChildNodes[1].ChildNodes[0].OuterXml;

                s = new TVShow(TVShowID, SeriesName, SeriesPosterUrl, SeriesBannerUrl);

                s.Network = nodeList[0].SelectSingleNode("Network").InnerText;
                s.Rating = nodeList[0].SelectSingleNode("Rating").InnerText;
                s.Overview = nodeList[0].SelectSingleNode("Overview").InnerText;
                s.Runtime = nodeList[0].SelectSingleNode("Runtime").InnerText;
                s.Genre = nodeList[0].SelectSingleNode("Genre").InnerText;
                s.FirstAired = nodeList[0].SelectSingleNode("FirstAired").InnerText;
                s.Actors = nodeList[0].SelectSingleNode("Actors").InnerText;

                //Deal with the XML for specific episodes
                nodeList = node.SelectNodes("/Data/Episode");

                foreach (XmlNode x in nodeList)
                {

                    //Extract metadata for episode/seasons
                    String EpisodeXML = "<Item>" + x.InnerXml + "</Item>";
                    String EpisodeID = x["id"].InnerText;
                    Int32 SeasonNumber = Int32.Parse(x["SeasonNumber"].InnerText);
                    Int32 EpisodeNumber = Int32.Parse(x["EpisodeNumber"].InnerText);
                    String EpisodeName = x["EpisodeName"].InnerText;
                    String EpisodePosterURL = x["filename"].InnerText;
                    String Overview = x["Overview"].InnerText;

                    if (!s.Seasons.ContainsKey(SeasonNumber))
                    {
                        s.Seasons.Add(SeasonNumber, new Season(SeasonNumber, TVShowID));
                    }

                    if (!s.Seasons[SeasonNumber].Episodes.ContainsKey(EpisodeNumber))
                    {
                        Episode ep = new Episode(EpisodeNumber, EpisodeName, ((String.IsNullOrEmpty(EpisodePosterURL)) ? String.Empty :urlPoster + EpisodePosterURL));
                        ep.Overview = Overview;
                        ep.EpisodeID = EpisodeID;
                        s.Seasons[SeasonNumber].Episodes.Add(EpisodeNumber, ep);
                    }
                }

                //Cache metadata
                try
                {
                    Message("Caching Metadata", MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);
                    xdoc.Save(CacheDirectory + "\\" + TVShowID + ".xml");
                }
                catch (Exception ex)
                {
                    Message("Error caching metadata: " + ex.Message, MediaScoutMessage.MessageType.Error, DateTime.Now);
                }


            }
            catch (Exception ex)
            {
                s = null;
                throw new Exception(ex.Message + ex.StackTrace);
            }

            return s;

        }


        public Posters[] GetPosters(String TVShowID, TVShowPosterType type, String season)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(urlMetadata + TVShowID + "/banners.xml");
            XmlNodeList xnl = xdoc.DocumentElement.SelectNodes("/Banners/Banner");

            //List<String> posters = new List<string>();
            List<Posters> posters = new List<Posters>();

            String t = StringEnum.GetStringValue(type);
            foreach (XmlNode x in xnl)
            {
                Debug.WriteLine(x.InnerXml.ToString());
                if (x.SelectSingleNode("BannerType").InnerText == t)
                {
                    if (type == TVShowPosterType.Season)
                    {
                        if (x.SelectSingleNode("Season").InnerText != season)
                            continue;
                    }

                    Posters p = new Posters()
                    {
                        Poster = urlPoster + x.SelectSingleNode("BannerPath").InnerText,
                        Thumb = (x.SelectSingleNode("ThumbnailPath") != null) ? urlPoster + x.SelectSingleNode("ThumbnailPath").InnerText : null,
                        Resolution = (x.SelectSingleNode("BannerType2") != null) ? x.SelectSingleNode("BannerType2").InnerText : null
                    };

                    posters.Add(p);
                }
            }

            return posters.ToArray();
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

    public class StringValueAttribute : System.Attribute
    {
        private string _value;

        public StringValueAttribute(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    public enum TVShowPosterType
    {
        [StringValue("poster")]
        Poster,

        [StringValue("fanart")]
        Backdrop,

        [StringValue("series")]
        Banner,

        [StringValue("season")]
        Season
    }

    public static class StringEnum
    {
        private static Hashtable _stringValues = new Hashtable();

        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            //Check first in our cached results...

            if (_stringValues.ContainsKey(value))
                output = (_stringValues[value] as StringValueAttribute).Value;
            else
            {
                //Look for our 'StringValueAttribute' 

                //in the field's custom attributes

                FieldInfo fi = type.GetField(value.ToString());
                StringValueAttribute[] attrs =
                   fi.GetCustomAttributes(typeof(StringValueAttribute),
                                           false) as StringValueAttribute[];
                if (attrs.Length > 0)
                {
                    _stringValues.Add(value, attrs[0]);
                    output = attrs[0].Value;
                }
            }

            return output;
        }
    }

}
