using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace MediaScout.Providers
{
    public class TheMovieDBProvider_21 : IMovieMetadataProvider
    {
        private static String APIKey = "1a9efd23fff9c2ed07c90358e2b3d280";
        private static String osUri = "http://a9.com/-/spec/opensearch/1.1/";
        private String urlSearchMovie = string.Format("http://api.themoviedb.org/2.1/Movie.search/{0}/xml/{1}/", "en", APIKey);
        private String urlMovieInfo = string.Format("http://api.themoviedb.org/2.1/Movie.getInfo/{0}/xml/{1}/", "en", APIKey);

        private String defaultCacheDir = System.Environment.CurrentDirectory + @"\Cache\MovieCache\";
        private DateTime dtDefaultCache = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0)); //Refresh Interval 14 days

        #region IMovieMetadataProvider Members

        public Movie[] Search(string MovieName)
        {
            //TheMovieDB doesn't handle & very well, so convert to "AND"
            MovieName = MovieName.Replace("&", "and");
            MovieName = MovieName.Replace(" ", "+");

            XmlDocument xdoc = new XmlDocument();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("opensearch", osUri);
            xdoc.Load(urlSearchMovie + MovieName);
            XmlNode node = xdoc.DocumentElement;
            XmlNodeList xnl = node.SelectNodes("./movies/movie");

            List<Movie> movies = new List<Movie>();
            for (int i = 0; i < xnl.Count; i++)
            {
                Movie m = new Movie();
                if (xnl[i]["name"] != null)
                    m.Title = xnl[i]["name"].InnerText;

                if (xnl[i]["id"] != null)
                    m.ID = xnl[i]["id"].InnerText;

                if (xnl[i]["overview"] != null)
                    m.Description = xnl[i]["overview"].InnerText;

                if (xnl[i].SelectSingleNode("./images/image[@type='poster'][@size='original']") != null)
                    m.Poster = xnl[i].SelectSingleNode("./images/image[@type='poster'][@size='original']").Attributes["url"].Value;

                if (xnl[i].SelectSingleNode("./images/image[@type='poster'][@size='thumb']") != null)
                    m.Thumbnail = xnl[i].SelectSingleNode("./images/image[@type='poster'][@size='thumb']").Attributes["url"].Value;

                movies.Add(m);
            }

            return movies.ToArray();
        }

        public Movie Get(string MovieID)
        {
            XmlNode node;
            XmlDocument xdoc = new XmlDocument();

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("opensearch", osUri);
            if (File.Exists(defaultCacheDir + "\\" + MovieID + ".xml") && (DateTime.Compare(File.GetLastWriteTime(defaultCacheDir + "\\" + MovieID + ".xml"), dtDefaultCache) > 0))
            {
                xdoc.Load(defaultCacheDir + "\\" + MovieID + ".xml");
            }
            else
            {
                xdoc.Load(urlMovieInfo + MovieID);
            }
            node = xdoc.DocumentElement;
            XmlNodeList nlMovie = node.SelectNodes("./movies/movie");

            if (nlMovie[0].FirstChild == null)
                throw new Exception("no results");

            Movie m = new Movie();

            if (nlMovie[0].SelectSingleNode("released") != null)
                try
                {
                    m.ProductionYear = nlMovie[0].SelectSingleNode("released").InnerText.Substring(0, 4);
                }
                catch (Exception ex)
                {

                }
            if (nlMovie[0].SelectSingleNode("overview") != null)
                m.Description = nlMovie[0].SelectSingleNode("overview").InnerText;

            if (nlMovie[0].SelectSingleNode("runtime") != null)
                m.RunningTime = nlMovie[0].SelectSingleNode("runtime").InnerText;

            //if (nlMovie[0].SelectSingleNode("poster[@size='original']") != null)
            //    m.Poster = nlMovie[0].SelectSingleNode("poster[@size='original']").InnerText;
            if (nlMovie[0].SelectSingleNode("./images/image[@type='poster'][@size='original']") != null)
                m.Poster = nlMovie[0].SelectSingleNode("./images/image[@type='poster'][@size='original']").Attributes["url"].Value;

            //if (nlMovie[0].SelectSingleNode("backdrop[@size='original']") != null)
            //    m.Backdrop = nlMovie[0].SelectSingleNode("backdrop[@size='original']").InnerText;
            if (nlMovie[0].SelectSingleNode("./images/image[@type='backdrop'][@size='original']") != null)
                m.Backdrop = nlMovie[0].SelectSingleNode("./images/image[@type='backdrop'][@size='original']").Attributes["url"].Value;

            if (nlMovie[0].SelectSingleNode("rating") != null)
                m.Rating = nlMovie[0].SelectSingleNode("rating").InnerText;

            m.Title = nlMovie[0].SelectSingleNode("name").InnerText;
            m.ID = (nlMovie[0].SelectSingleNode("id") == null) ? nlMovie[0].SelectSingleNode("TMDbId").InnerText : nlMovie[0].SelectSingleNode("id").InnerText;

            //Get the actors/directors/etc
            XmlNodeList nlActors = node.SelectNodes("./movies/movie/cast/person");
            foreach (XmlNode x in nlActors)
            {
                m.Persons.Add(new Person()
                {
                    Name = x.Attributes["name"].Value,
                    Type = x.Attributes["job"].Value,
                    Role = (x.Attributes["character"].Value == null) ? "" : x.Attributes["character"].Value.Trim()
                });
            }

            //Get the genres
            XmlNodeList nlGenres = node.SelectNodes("./movies/movie/categories/category");
            foreach (XmlNode x in nlGenres)
                m.Genres.Add(new Genre() { name = x.Attributes["name"].Value });

            //Cache metadata
            try
            {
                // Message("Caching Metadata", MediaScoutMessage.MessageType.ProcessSeries, DateTime.Now);
                xdoc.Save(defaultCacheDir + "\\" + MovieID + ".xml");
            }
            catch (Exception ex)
            {
                //Message("Error caching metadata: " + ex.Message, MediaScoutMessage.MessageType.Error, DateTime.Now);
            }

            return m;
        }

        public Posters[] GetPosters(String MovieID, MoviePosterType type)
        {
            string selectImages = string.Format("./images/image[@type='{0}'][@size='original']", StringEnum.GetStringValue(type)); 
            string selectImageThumbnail = string.Format("./images/image[@type='{0}'][@size='thumb']", StringEnum.GetStringValue(type)); 
            XmlDocument xdoc = new XmlDocument();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("opensearch", osUri);
            xdoc.Load(urlMovieInfo + MovieID);

            XmlNodeList nlMovie = xdoc.DocumentElement.SelectNodes("./movies/movie");

            if (nlMovie[0].FirstChild == null)
                throw new Exception("no results");

            List<Posters> posters = new List<Posters>();

            XmlNodeList xnl = nlMovie[0].SelectNodes(selectImages);
            

            //if (nlMovie[0].SelectSingleNode("./images/image[@type='poster'][@size='original']") != null)
            //    m.Poster = nlMovie[0].SelectSingleNode("./images/image[@type='poster'][@size='original']").Attributes["url"].Value;

            foreach (XmlNode x in xnl)
            {
                string posterUrl = x.Attributes["url"].Value;
                string thumbnailUrl = nlMovie[0].SelectSingleNode(string.Format(selectImageThumbnail+ "[@id='{0}']",x.Attributes["id"].Value)).Attributes["url"].Value;
                Posters p = new Posters()
                {
                    Poster = posterUrl,
                    Thumb = thumbnailUrl
                };
                posters.Add(p);
            }

            //if (posters.Count == 0)
            //{
            //    xnl = nlMovie[0].SelectNodes(t + "[@size='mid']");

            //    foreach (XmlNode x in xnl)
            //    {
            //        posters.Add(new Posters() { Poster = x.InnerText });
            //    }
            //}


            return posters.ToArray();
        }

        #endregion

        #region IMetadataProvider Members
        public string name
        {
            get { return "The Movie DB"; }
        }

        public string version
        {
            get { return "2.1"; }
        }

        public string url
        {
            get { return ""; }
        }

        #endregion
    }
}
