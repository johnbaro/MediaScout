using System;
using System.Collections.Generic;
using System.Text;
using MediaScout.Providers;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace MediaScout.Providers
{
    public class TheMovieDBProvider : IMovieMetadataProvider
    {

        string IMetadataProvider.name { get { return "The Movie DB"; } }
        string IMetadataProvider.version { get { return "0.1"; } }
        string IMetadataProvider.url { get { return ""; } }


        private static String APIKey = "1a9efd23fff9c2ed07c90358e2b3d280";
        private static String osUri = "http://a9.com/-/spec/opensearch/1.1/";
        private String urlSearchMovie = @"http://api.themoviedb.org/2.0/Movie.search?api_key=" + APIKey + "&title=";
        private String urlMovieInfo = @"http://api.themoviedb.org/2.0/Movie.getInfo?api_key=" + APIKey + "&id=";
        private String defaultCacheDir = System.Environment.CurrentDirectory + @"\Cache\MovieCache\";
        private DateTime dtDefaultCache = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));

        /// <summary>
        /// Search for a movie title via TheMovieDB's database
        /// </summary>
        /// <param name="MovieName">Movie title to search for (ie, "The Matrix")</param>
        /// <returns>Search results containing ID/title/description/poster (optional) in an array of MediaScout.Movie</returns>
        public Movie[] Search(string MovieName)
        {
            //TheMovieDB doesn't handle & very well, so convert to "AND"
            MovieName = MovieName.Replace("&", "and");


            XmlDocument xdoc = new XmlDocument();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("opensearch", osUri);
            xdoc.Load(urlSearchMovie + MovieName);
            XmlNode node = xdoc.DocumentElement;
            XmlNodeList xnl = node.SelectNodes("/results/moviematches/movie");

            List<Movie> movies = new List<Movie>();
            for (int i = 0; i <xnl.Count; i++)
            {
                Movie m = new Movie();
                if (xnl[i]["title"] != null)
                    m.Title = xnl[i]["title"].InnerText;

                if (xnl[i]["id"] != null)
                    m.ID = xnl[i]["id"].InnerText;

                if (xnl[i]["short_overview"] != null)
                    m.Description = xnl[i]["short_overview"].InnerText;

                if (xnl[i].SelectSingleNode("poster[@size='original']") != null)
                    m.Poster = xnl[i].SelectSingleNode("poster[@size='original']").InnerText;

                if (xnl[i].SelectSingleNode("poster[@size='thumb']") != null)
                    m.Thumbnail = xnl[i].SelectSingleNode("poster[@size='thumb']").InnerText;
                
                movies.Add(m);
            }

            return movies.ToArray();
        }

        /// <summary>
        /// Returns the full results of a given movie id obtained with TheMovieDBProvider.Search
        /// </summary>
        /// <param name="MovieID">MovieID (Usually int)</param>
        /// <returns>Movie object, ready to be serialised to XML</returns>
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
            XmlNodeList nlMovie = node.SelectNodes("/results/moviematches/movie");

            if (nlMovie[0].FirstChild == null)
                throw new Exception("no results");

            Movie m = new Movie();

            if (nlMovie[0].SelectSingleNode("release") != null)
                try
                {
                    m.ProductionYear = nlMovie[0].SelectSingleNode("release").InnerText.Substring(0, 4);
                }
                catch (Exception ex)
                {

                }
            if (nlMovie[0].SelectSingleNode("short_overview") != null)
                m.Description = nlMovie[0].SelectSingleNode("short_overview").InnerText;

            if (nlMovie[0].SelectSingleNode("runtime") != null)
                m.RunningTime = nlMovie[0].SelectSingleNode("runtime").InnerText;
            
            if (nlMovie[0].SelectSingleNode("poster[@size='original']") != null)
                m.Poster = nlMovie[0].SelectSingleNode("poster[@size='original']").InnerText;

            if (nlMovie[0].SelectSingleNode("backdrop[@size='original']") != null)
                m.Backdrop = nlMovie[0].SelectSingleNode("backdrop[@size='original']").InnerText;

            if (nlMovie[0].SelectSingleNode("rating") != null)
                m.Rating = nlMovie[0].SelectSingleNode("rating").InnerText;

            m.Title = nlMovie[0].SelectSingleNode("title").InnerText;
            m.ID = (nlMovie[0].SelectSingleNode("id") == null) ? nlMovie[0].SelectSingleNode("TMDbId").InnerText : nlMovie[0].SelectSingleNode("id").InnerText;

            //Get the actors/directors/etc
            XmlNodeList nlActors = node.SelectNodes("/results/moviematches/movie/people/person");
            foreach (XmlNode x in nlActors)
            {
                m.Persons.Add(new Person()
                {
                    Name = x.SelectSingleNode("name").InnerText,
                    Type = x.Attributes["job"].Value,
                    Role = (x.SelectSingleNode("role").InnerText == null) ? "" : x.SelectSingleNode("role").InnerText.Trim()
                });
            }

            //Get the genres
            XmlNodeList nlGenres = node.SelectNodes("/results/moviematches/movie/categories/category");
            foreach (XmlNode x in nlGenres)
                m.Genres.Add(new Genre() { name = x.SelectSingleNode("name").InnerText });

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
            String t = StringEnum.GetStringValue(type);
            XmlDocument xdoc = new XmlDocument();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("opensearch", osUri);
            xdoc.Load(urlMovieInfo + MovieID);


            XmlNodeList nlMovie = xdoc.DocumentElement.SelectNodes("/results/moviematches/movie");

            if (nlMovie[0].FirstChild == null)
                throw new Exception("no results");

            List<Posters> posters = new List<Posters>();

            XmlNodeList xnl = nlMovie[0].SelectNodes(t + "[@size='original']");

            foreach (XmlNode x in xnl)
            {
                Posters p = new Posters()
                {
                    Poster = x.InnerText
                };
                posters.Add(p);
            }

            if (posters.Count == 0)
            {
                xnl = nlMovie[0].SelectNodes(t + "[@size='mid']");

                foreach (XmlNode x in xnl)
                {
                    posters.Add(new Posters() { Poster = x.InnerText });
                }
            }


            return posters.ToArray();
        }
    }
    public enum MoviePosterType
    {
        [StringValue("poster")]
        Poster,

        [StringValue("backdrop")]
        Backdrop,
    }
}
