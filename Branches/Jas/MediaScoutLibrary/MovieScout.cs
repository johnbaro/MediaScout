using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;
using System.Net;

using MediaScout.Providers;

namespace MediaScout
{
    public class MovieScout
    {
        /// <summary>
        /// Saves a MediaScout.Movie to a serialised XML format, usable by MyMovies or VideoBrowser
        /// </summary>
        /// <param name="m">Movie to serialise</param>
        /// <param name="file">File to save it to</param>
        public void SaveMovie(Movie m, String file)
        {
            XmlSerializer s = new XmlSerializer(typeof(Movie));
            TextWriter w = new StreamWriter(file);
            s.Serialize(w, m);
            w.Close();
        }

        /// <summary>
        /// Downloads and saves a MediaScout.Movie poster to file
        /// </summary>
        /// <param name="m">movie to save poster from</param>
        /// <param name="file">file to save to</param>
        public void SavePoster(String poster, String file)
        {
            try
            {
                // Create the requests.
                WebRequest requestPic = WebRequest.Create(poster);
                WebResponse responsePic = requestPic.GetResponse();
                System.Drawing.Image p = System.Drawing.Image.FromStream(responsePic.GetResponseStream());
                p.Save(file);
            }
            catch
            {

            }
        }

        private TheMovieDBProvider_21 tmdb = new TheMovieDBProvider_21();

        public MovieScout()
        {

        }

        public Movie[] Search(String SearchString)
        {
            Movie[] results = tmdb.Search(SearchString);

            return results;
        }

        public Movie Get(String ID)
        {
            Movie results = tmdb.Get(ID);

            return results;
        }
    }
}
