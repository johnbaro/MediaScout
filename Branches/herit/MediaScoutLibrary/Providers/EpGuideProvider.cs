using System;
using System.Collections.Generic;
using MediaScout.Providers;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace MediaScout.Providers
{
    public class EpGuideProvider : ITVMetadataProvider
    {
        public event MediaScoutMessage.Message Message;

        private String urlSeriesID = @"http://epguides.com/";
        //private String urlMetadata = @"http://epguides.com/";
        private String defaultLanguage = "en";
        private String EpGuidePage = ""; // sort of used in place of urlMetadata, .Search() populates it

        public string name { get { return "EpGuides"; } }
        public string version { get { return "0.1"; } }
        public string url { get { return "http://www.EpGuides.com"; } }

        public String defaultCacheDir =  System.Environment.CurrentDirectory  + @"\Cache\TVCache\";
        public DateTime dtDefaultCache = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));

        //EpGuides does not have a real search feature, they just use google.
        public TVShow[] Search(String SeriesName)
        {
            return Search(SeriesName, defaultLanguage);
        }

        public TVShow[] Search(String SeriesName, String Language)
        {
            try
            {
                //Create the SeriesID based on a few assumptions (alpha numeric only, no spaces, etc)
                String SeriesID = SeriesName;
                SeriesID = SeriesID.Replace(" ", "");
                SeriesID = SeriesID.Replace(".", "");
                SeriesID = SeriesID.Replace("'", "");
                SeriesID = SeriesID.Replace(",", "");

                Debug.WriteLine("Fetching EpList from " + urlSeriesID + SeriesID + " with HttpWebRequest");
                // http://epguides.com/common/preferences.asp
                // epguides can show listings managed by TV.com or by TVRage.  The TV.com list returns
                // ASCII or ISO-8859-1 encoded text but the TVRage lists are UTF-8 and my RegEx cannot
                // match against it.  Thankfully, epguides allows you set a prefrence as to which list
                // you want to see and it generates a cookie for that.  So I can generate that same cookie
                // here to make sure we always get the TV.com list.
                System.Net.HttpWebRequest req1 = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(urlSeriesID + SeriesID);
                System.Net.Cookie ckie = new System.Net.Cookie();
                ckie.Name = "ListDisplay";
                ckie.Value = "tv.com";
                ckie.Path = "/";
                ckie.Domain = "epguides.com";
                req1.CookieContainer = new System.Net.CookieContainer();
                req1.CookieContainer.Add(ckie);
                System.Net.HttpWebResponse rsp1 = ((System.Net.HttpWebResponse)(req1.GetResponse()));

                // I need to do some error handeling when a series is not found (http 404 errors, etc)

                System.IO.StreamReader reader1 = new System.IO.StreamReader(rsp1.GetResponseStream());
                EpGuidePage = reader1.ReadToEnd();
                reader1.Close();

                TVShow t = new TVShow();
                t.id = SeriesID;
                t.SeriesID = SeriesID;

                // Do some new pattern matching to pull the following info from the HTML
                Regex rxEpGuide;
                Match mEpGuide;
                // First see if we can pull the series name out of the HTML Page Title tag.
                rxEpGuide = new Regex(@"<title>(?<SeriesName>.+) \(a Titles \&amp\; Air Dates Guide\)<\/title>", RegexOptions.Multiline);
                mEpGuide = rxEpGuide.Match(EpGuidePage);
                // I hate this nested IF, but I can't think right now.
                if (mEpGuide.Success)
                {
                    t.SeriesName = mEpGuide.Groups["SeriesName"].Value.Trim();
                }
                else
                {
                    // Try pulling it from the <h1> tag which seems to only be used for the series title
                    rxEpGuide = new Regex(@"<h1><a href.+>(?<SeriesName>.+)<\/a><\/h1>", RegexOptions.Multiline);
                    if (mEpGuide.Success)
                    {
                        t.SeriesName = mEpGuide.Groups["SeriesName"].Value.Trim();
                    }
                }

                List<TVShow> tvshows = new List<TVShow>();
                tvshows.Add(t);

                return tvshows.ToArray();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception Caught: " + e);
                return null;
            }
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
            TVShow s = new TVShow();

            if (EpGuidePage == "")
            {
                Debug.WriteLine("Must use Search first to fetch the EpGuide HTML page!");
                s = null;
                return s;
            }

            if (CacheDirectory == null)
                CacheDirectory = defaultCacheDir;

            if (dtCacheTime == null)
                dtCacheTime = dtDefaultCache;

            if (Language == null)
                Language = defaultLanguage;

            try
            {
                s = new TVShow(TVShowID, null, null, null);

                Debug.WriteLine("Page size: " + EpGuidePage.Length);
                Regex rxEpGuide = new Regex(@"(?<SeriesEp>.{5})\s(?<SeasonEp>.{6})\s(?<ProdNum>.{12})\s(?<AirDate>.{11})\s+<a .+href=\""(?<GuideURL>.+)?\"">(?<EpName>.+)<\/a>", RegexOptions.Multiline);
                Match mEpGuide = rxEpGuide.Match(EpGuidePage);

                int epCount = 0;
                while (mEpGuide.Success)
                {
                    epCount++;
                    Episode ep = new Episode();
                    Int32 SeasonNumber;

                    char[] splitchar = { '-' };
                    String[] _SE = mEpGuide.Groups["SeasonEp"].Value.Trim().Split(splitchar, 2);
                    if (_SE.Length == 2)
                    {
                        //This is a neat trick.  Set the SeasonNumber to 0 by default.  This is typically what
                        // people use for the "Specials" season anyway.  Next, try to conver the ep.SeasonNumber
                        // to an Int. If it fails, SeasonNumber stays 0, if it work it gets set to the true number
                        Int32.TryParse(_SE[0], out SeasonNumber);
                        ep.EpisodeNumber = _SE[1];
                    }
                    else // must not have found a '-', lt's pretend it's a special then
                    {
                        SeasonNumber = 0;
                        ep.EpisodeNumber = _SE[0];
                    }
                    ep.SeasonNumber = SeasonNumber.ToString();
                    ep.EpisodeName = mEpGuide.Groups["EpName"].Value.Trim();
                    ep.EpisodeID = mEpGuide.Groups["SeriesEp"].Value.Trim();
                    ep.FirstAired = mEpGuide.Groups["AirDate"].Value.Trim();
                    ep.ProductionCode = mEpGuide.Groups["ProdNum"].Value.Trim();

                    if (!s.Seasons.ContainsKey(SeasonNumber))
                    {
                        s.Seasons.Add(SeasonNumber, new Season(SeasonNumber, TVShowID));
                    }

                    // We're just going to assume we should add every episode we match, so fake our own key.
                    s.Seasons[SeasonNumber].Episodes.Add(epCount, ep);
                    //if (!s.Seasons[SeasonNumber].Episodes.ContainsKey(EpisodeNumber))
                    //{
                    //    s.Seasons[SeasonNumber].Episodes.Add(EpisodeNumber, ep);
                    //}

                    mEpGuide = mEpGuide.NextMatch();
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
